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

    string[] eyeHeader = { "eye_tracker_time", "eye_time_tick", "FPOGX", "FPOGY", "FPOGS", "FPOGD", "FPOGID", "FPOGV", "BPOGX", "BPOGY", "BPOGV" };

    char[] buffer = new char[4096];



    // Update is called once per frame
    void Update()
    {
        if ( this.IsConnected() && this.HasActiveLog())
        {
            int lines = 0;

            while (gazeStream.DataAvailable)
            {
                LogNextLine();
                lines++;
            }
            eyeDataWriter.Flush();

            Debug.Log(lines + "eyeLines this frame.");
        }
    }


 
    public void Connect()
    {
        gazeSocket = new TcpClient("127.0.0.1", 4242)
        {
            //buffer size (bytes) set for 40mb 
            ReceiveBufferSize = 40000000
        };

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
        gazeWriter.Write(GazeMsg.data_halt);
        gazeWriter.Flush();

        //this clears the buffer. Use with caution as fragments of half-written xlm message will still arive,
        // if stream has not been halter prior to the clearance
        //eyeReader.Read(buffer, 0, buffer.Length);
    }


    public bool IsConnected()
    {
        return gazeSocket != null;
    }


    public void Calibrate(bool manualSetup)
    {
        if (manualSetup)
        {
            // Manual Calibration
            //needs to hide the display first, in order to be able to maximize it after, in case display was in the background and not minimized
            gazeWriter.Write(GazeMsg.display_hide);
            gazeWriter.Flush();
            System.Threading.Thread.Sleep(200);
            gazeWriter.Write(GazeMsg.display_show);
            gazeWriter.Flush();
        }
        else
        {
            //Automatic calibration setup: Shows calibration window and begins calibration
            gazeWriter.Write(GazeMsg.calibration_show);
            gazeWriter.Write(GazeMsg.calibration_start);
            gazeWriter.Write(GazeMsg.display_hide);
            gazeWriter.Flush();
        }
    }


    public void StartNewLog()
    {
        if (gazeSocket != null)
        {
            string fileRootName = string.Format("{0}_{1}", Settings.subjectID, System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
            eyeDataWriter = new StreamWriter(Settings.subjectDir + "/" + fileRootName + "_eye.tsv", true);
            eyeDataWriter.WriteLine("Meta-Two build\t" + System.IO.File.GetLastWriteTime(System.Reflection.Assembly.GetExecutingAssembly().Location));
            eyeDataWriter.WriteLine("Exp. Start Time\t" + Settings.expStartTime.ToString("yyyy-MM-dd_HH-mm-ss"));
            eyeDataWriter.WriteLine("SID\t" + Settings.subjectID);
            eyeDataWriter.WriteLine("ECID\t" + Settings.ECID);
            eyeDataWriter.WriteLine("Session\t" + Settings.session);
            eyeDataWriter.WriteLine("Screen resolution\t" + Screen.currentResolution);
            eyeDataWriter.WriteLine("Screen dpi\t" + Screen.dpi);
            eyeDataWriter.WriteLine("Fullscreen\t" + Screen.fullScreen);
            eyeDataWriter.WriteLine("Window height\t" + Screen.height);
            eyeDataWriter.WriteLine("Window width\t" + Screen.width);
            eyeDataWriter.WriteLine(string.Join("\t", eyeHeader));

            // clears up buffer from previous loggings
            gazeReader.Read(buffer, 0, buffer.Length);

            gazeWriter.Write(GazeMsg.tickfrequency_get);
            gazeWriter.Write(GazeMsg.calibration_getPts);
            gazeWriter.Write(GazeMsg.data_send);
            gazeWriter.Flush();
        }
    }


    public bool HasActiveLog()
    {
        return this.eyeDataWriter != null;
    }


    void LogNextLine()
    {
            string eyeLine = gazeReader.ReadLine();


        //if it is a data-line...
        //TODO: this condition is impossible, implement converting lines into array 
        if (eyeLine.StartsWith("<REC TIME") && eyeLine == null)
            {
                Debug.Log("impossibrulity!");
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

                eyeDataWriter.WriteLine(string.Join("\t", relevantData));
            }
            else
            {
                //... just write is as the raw string
                eyeDataWriter.WriteLine(eyeLine);
                eyeDataWriter.Flush();
            }
    }


    public void FinishLog()
    {
        gazeWriter.Write(GazeMsg.data_halt);
        gazeWriter.Flush();

        eyeDataWriter.Flush();
        eyeDataWriter.Close();
        eyeDataWriter = null;
    }

}
