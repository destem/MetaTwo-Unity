using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.IO;


public enum EyeData { ETT, TIMETICK, FPOGX, FPOGY, FPOGS, FPOGD, FPOGID, FPOGV, BPOGX, BPOGY, BPOGV };

public class EyeTrackerScript : MonoBehaviour
{

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
            gazeWriter.Write("<GET ID=\"TIME_TICK_FREQUENCY\" />\r\n");
            gazeWriter.Write("<GET ID=\"CALIBRATE_ADDPOINT\" />\r\n");
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

            gazeWriter.Write("<SET ID=\"ENABLE_SEND_TIME\" STATE=\"1\" />\r\n");
            gazeWriter.Write("<SET ID=\"ENABLE_SEND_POG_FIX\" STATE=\"1\" />\r\n");
            gazeWriter.Write("<SET ID=\"ENABLE_SEND_POG_BEST\" STATE=\"1\" />\r\n");
            gazeWriter.Write("<SET ID=\"ENABLE_SEND_TIME_TICK\" STATE=\"1\" />\r\n");
            gazeWriter.Write("<SET ID=\"ENABLE_SEND_DATA\" STATE=\"1\" />\r\n");
            gazeWriter.Flush();

            // Manual Calibration
            gazeWriter.Write("<SET ID=\"TRACKER_DISPLAY\" STATE=\"0\" />\r\n");
            gazeWriter.Flush();
            System.Threading.Thread.Sleep(200);
            gazeWriter.Write("<SET ID=\"TRACKER_DISPLAY\" STATE=\"1\" />\r\n");
            gazeWriter.Flush();


            //Automatic calibration setup: Shows calibration window and begins calibration
            //gazeWriter.Write("<SET ID=\"CALIBRATE_SHOW\" STATE=\"1\" />\r\n");
            //gazeWriter.Write("<SET ID=\"CALIBRATE_START\" STATE=\"1\" />\r\n");
            //gazeWriter.Write("<SET ID=\"TRACKER_DISPLAY\" STATE=\"0\" />\r\n");
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
