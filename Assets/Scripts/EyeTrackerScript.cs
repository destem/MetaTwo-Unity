using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.IO;


public enum EyeData { ETT, TIMETICK, FPOGX, FPOGY, FPOGS, FPOGD, FPOGID, FPOGV, BPOGX, BPOGY, BPOGV };

public class EyeTrackerScript : MonoBehaviour
{

    public static class GazeMsg
    {
        public static string display_show = "<SET ID=\"TRACKER_DISPLAY\" STATE=\"1\" />\r\n";
        public static string display_hide = "<SET ID=\"TRACKER_DISPLAY\" STATE=\"0\" />\r\n";

        public static string enable_time = "<SET ID=\"ENABLE_SEND_TIME\" STATE=\"1\" />\r\n";
        public static string enable_tick = "<SET ID=\"ENABLE_SEND_TIME_TICK\" STATE=\"1\" />\r\n";
        public static string enable_pogFix = "<SET ID=\"ENABLE_SEND_POG_FIX\" STATE=\"1\" />\r\n";

        public static string enable_pogLeft = "<SET ID=\"ENABLE_SEND_POG_LEFT\" STATE=\"1\" />\r\n";
        public static string enable_pogRight = "<SET ID=\"ENABLE_SEND_POG_RIGHT\" STATE=\"1\" />\r\n";
        public static string enable_pogBest = "<SET ID=\"ENABLE_SEND_POG_BEST\" STATE=\"1\" />\r\n";

        public static string enable_pupilLeft = "<SET ID=\"ENABLE_SEND_PUPIL_LEFT\" STATE=\"1\" />\r\n";
        public static string enable_pupilRight = "<SET ID=\"ENABLE_SEND_PUPIL_RIGHT\" STATE=\"1\" />\r\n";

        public static string enable_eyeLeft = "<SET ID=\"ENABLE_SEND_EYE_LEFT\" STATE=\"1\" />\r\n";
        public static string enable_eyeRight = "<SET ID=\"ENABLE_SEND_EYE_RIGHT\" STATE=\"1\" />\r\n";

        public static string enable_Cursor = "<SET ID=\"ENABLE_SEND_CURSOR\" STATE=\"1\" />\r\n";

        public static string data_send = "<SET ID=\"ENABLE_SEND_DATA\" STATE=\"1\" />\r\n";
        public static string data_halt = "<SET ID=\"ENABLE_SEND_DATA\" STATE=\"0\" />\r\n";

        public static string calibration_show = "<SET ID=\"CALIBRATE_SHOW\" STATE=\"1\" />\r\n";
        public static string calibration_start = "<SET ID=\"CALIBRATE_START\" STATE=\"1\" />\r\n";
        public static string calibration_getPts = "<GET ID=\"CALIBRATE_ADDPOINT\" />\r\n";

        public static string tickfrequency_get = "<GET ID=\"TIME_TICK_FREQUENCY\" />\r\n";


        public static string SetID(string id, bool enabled)
        {
            int val = 1;
            if (!enabled)
            {
                val = 0;
            }
            return "<SET ID=\"" + id + "\" STATE=\"" + val + "\" />\r\n";
        }

        public static string getID(string id)
        {
            return "<GET ID=\"" + id + "\" />\r\n";
        }
    }


    TcpClient gazeSocket;
    NetworkStream gazeStream;
    StreamReader gazeReader;
    StreamWriter gazeWriter;
    StreamWriter eyeDataWriter;
    bool evenFrame = false;

    string[] eyeHeader = { "eye_tracker_time", "eye_time_tick", "FPOGX", "FPOGY", "FPOGS", "FPOGD", "FPOGID", "FPOGV", "BPOGX", "BPOGY", "BPOGV" };

    char[] buffer = new char[4096];

    bool finishing = false;



    // Update is called once per frame
    void Update()
    {
        if (gazeStream != null && gazeStream.DataAvailable && eyeDataWriter != null && !finishing)
        {
            //Assumption: Gazepoint is running @150Hz, game @60Hz
            // --> 1000ms / 150Hz * 5 == 1000ms / 60 Hz * 2
            // ==> collect  5 eyedata lines every 2 frames

            logNextLine();
            logNextLine();

            if (evenFrame)
            {
                logNextLine();
            }

            evenFrame = !evenFrame;
        }
        //TODO: if game  not initialized, periodically clear buffer, to prevent big chunk of data?
    }



    // TcpClient.ReceiveBufferSize property

    public void ketchUp(long tick)
    {
        if (eyeDataWriter != null)
        {
            finishing = true;

            long gameTick = 0;

            while (gazeStream.DataAvailable && gameTick < tick && gazeReader != null && gazeStream != null)
            {
                gameTick = logNextLine();
            }
            eyeDataWriter.Flush();

            finishing = false;
        }
    }

    public void startNewLog()
    {
        if (gazeSocket != null)
        {
            string fileRootName = string.Format("{0}_{1}", Settings.subjectID, System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));


            eyeDataWriter = new StreamWriter(Settings.logDir + "/" + fileRootName + "_eye.tsv", true);
            eyeDataWriter.WriteLine(string.Join("\t", eyeHeader));
            gazeWriter.Write(GazeMsg.tickfrequency_get);
            gazeWriter.Write(GazeMsg.calibration_getPts);
            gazeWriter.Flush();
        }
    }


    public void ConnectToEyetrackerAndCalibrate()
    {
        gazeSocket = new TcpClient("127.0.0.1", 4242);
        if (gazeSocket != null)
        {
            gazeStream = gazeSocket.GetStream();
            gazeWriter = new StreamWriter(gazeStream);

            gazeReader = new StreamReader(gazeStream);

            gazeWriter.Write(GazeMsg.enable_time);
            gazeWriter.Write(GazeMsg.enable_tick);
            gazeWriter.Write(GazeMsg.enable_pogFix);
            gazeWriter.Write(GazeMsg.enable_pogBest);

            gazeWriter.Write(GazeMsg.enable_pogRight);
            gazeWriter.Write(GazeMsg.enable_pogLeft);
            gazeWriter.Write(GazeMsg.enable_pupilLeft);
            gazeWriter.Write(GazeMsg.enable_pupilRight);
            gazeWriter.Write(GazeMsg.enable_eyeLeft);
            gazeWriter.Write(GazeMsg.enable_eyeRight);

            gazeWriter.Write(GazeMsg.data_send);
            gazeWriter.Flush();

            // Manual Calibration
            //needs to hide the display first, in order to be able to maximize it after, in case display was in the background and not minimized
            gazeWriter.Write(GazeMsg.display_hide);
            gazeWriter.Flush();
            System.Threading.Thread.Sleep(200);
            gazeWriter.Write(GazeMsg.display_show);
            gazeWriter.Flush();


            //Automatic calibration setup: Shows calibration window and begins calibration
            //gazeWriter.Write(GazeMsg.calibration_show);
            //gazeWriter.Write(GazeMsg.calibration_start);
            //gazeWriter.Write(GazeMsg.display_hide);
            //gazeWriter.Flush();


            //this clears the buffer. Use with caution as fragments of half-written xlm message will still arive,
            // if stream has not been halter prior to the clearance
            //eyeReader.Read(buffer, 0, buffer.Length);
        }
    }



    long logNextLine()
    {
        long result = -1;

        string eyeLine = gazeReader.ReadLine();

        //if it is a data-line...
        if (eyeLine.StartsWith("<REC TIME") && eyeLine == null)
        {
            char[] separatingChars = { ' ', '=', '"' };
            //...spilt it...
            string[] splitEyeLine = eyeLine.Split(separatingChars); ;


            //TODO: implement this more elegant way
            foreach (string eyeFragment in splitEyeLine)
            {
                switch (eyeFragment)
                {

                    case "TIME":
                        //eyeString.add(eye.GetEnumerator().Current+2);
                        break;
                }
            }

            //..and extract the relevant cells
            string[] relevantData = {
                splitEyeLine[3],
                splitEyeLine[7],
                splitEyeLine[11],
                splitEyeLine[15],
                splitEyeLine[19],
                splitEyeLine[23],
                splitEyeLine[27],
                splitEyeLine[31],
                splitEyeLine[35],
                splitEyeLine[39],
                splitEyeLine[43]
            // Array Names
            //eyetrackingLogline.ETT = eye[3];
            //eyetrackingLogline.TIMETICK = eye[7];
            //eyetrackingLogline.FPOGX = eye[11];
            //eyetrackingLogline.FPOGY = eye[15];
            //eyetrackingLogline.FPOGS = eye[19];
            //eyetrackingLogline.FPOGD = eye[23];
            //eyetrackingLogline.FPOGID = eye[27];
            //eyetrackingLogline.FPOGV = eye[31];
            //eyetrackingLogline.BPOGX = eye[35];
            //eyetrackingLogline.BPOGY = eye[39];
            //eyetrackingLogline.BPOGV = eye[43];k
            };
            result = long.Parse(splitEyeLine[7]);

            eyeDataWriter.WriteLine(string.Join("\t", relevantData));
        }
        else
        {
            //... just write is as the raw string
            eyeDataWriter.WriteLine(eyeLine);
        }

        return result;
    }

}
