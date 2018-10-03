using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.IO;



public class EyeTrackerScript : MonoBehaviour {

    Game comp;
    public GameObject game;


    TcpClient socket;
    NetworkStream stream;
    StreamWriter writer;
    StreamReader reader;
    bool socketReady = false;
    char[] buffer = new char[4096];

    // Use this for initialization
    void Start () {
        comp = game.GetComponent<Game>();
    }
	

	// Update is called once per frame
	void Update () {

        if (socketReady && stream.DataAvailable && comp.log != null)
        {
            comp.log.eyetrackString = reader.ReadLine();
            //print(comp.log.eyetrackString);
            comp.log.LogEyeTracker();

            // clear read buffer. Eyetracker is running at roughly 60Hz, but just in case
            // NOTE: doing this keeps it in sync but misses every other frame, leading to one eyetrack event for every two game_state loglines
            //reader.Read(buffer, 0, buffer.Length);
        }
        //TODO: if game log not initialized, clear buffer, to prevent big chunk of data?
    }



    public void ConnectToEyetrackerAndCalibrate()
    {
        socketReady = true;
        socket = new TcpClient("127.0.0.1", 4242);
        stream = socket.GetStream();
        writer = new StreamWriter(stream);
        reader = new StreamReader(stream);

        writer.Write("<SET ID=\"ENABLE_SEND_TIME\" STATE=\"1\" />\r\n");
        writer.Write("<SET ID=\"ENABLE_SEND_POG_FIX\" STATE=\"1\" />\r\n");
        writer.Write("<SET ID=\"ENABLE_SEND_POG_BEST\" STATE=\"1\" />\r\n");
        writer.Write("<SET ID=\"ENABLE_SEND_TIME_TICK\" STATE=\"1\" />\r\n");
        //writer.Write("<SET ID=\"ENABLE_SEND_CURSOR\" STATE=\"1\" />\r\n");
        writer.Write("<SET ID=\"ENABLE_SEND_DATA\" STATE=\"1\" />\r\n");

        writer.Write("<SET ID=\"CALIBRATE_SHOW\" STATE=\"1\" />\r\n");
        writer.Flush();
        writer.Write("<SET ID=\"CALIBRATE_START\" STATE=\"1\" />\r\n");
        writer.Flush();
    }
}
