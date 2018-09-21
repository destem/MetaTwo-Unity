using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Net.Sockets;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class StartGame : MonoBehaviour
{
    
    public GameObject gameCanvas;
    public GameObject game;
    Game comp;
    Canvas canvas;
    bool acceptInput = true;

    TcpClient socket;
    NetworkStream stream;
    StreamWriter writer;
    StreamReader reader;
    bool socketReady = false;

    // Use this for initialization
    void Start()
    {
        comp = game.GetComponent<Game>();
        canvas = GetComponent<Canvas>();
    }


    // Update is called once per frame
    void Update()
    {
        if (acceptInput)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                comp.useKeyboard = true;
                comp.useTomee = false;
                comp.useRetro = false;
                LaunchGame();
            }
            if (Input.GetKeyDown("joystick button 0"))
            {
                //Assuming Tomee converted gamepad
                print("Tomee");
                comp.useKeyboard = false;
                comp.useTomee = true;
                comp.useRetro = false;
                LaunchGame();
            }
            if (Input.GetKeyDown("joystick button 1"))
            {
                //Assuming NES-Retro  gamepad
                print("Retro");
                comp.useKeyboard = false;
                comp.useTomee = false;
                comp.useRetro = true;
                LaunchGame();
            }
        }
        if (socketReady && stream.DataAvailable && comp.log != null)
        {
            //print(reader.ReadToEnd());
            //print("---------");
            //while (reader.Peek() > -1)
            //{
                comp.log.eyetrackString = reader.ReadLine();
                comp.log.LogEyeTracker();
            //}

            // EX: Server says: <REC TIME="38.41262" FPOGX="0.41352" FPOGY="0.80682" FPOGS="38.36427" FPOGD="0.04835" FPOGID="93" FPOGV="1" BPOGX="0.43701" BPOGY="0.81848" BPOGV="1" />
            //print("Server says: " + data);
        }
    }

 

    public void LaunchGame()
    {
        int level = System.Int32.Parse(transform.Find("InputField").gameObject.GetComponent<UnityEngine.UI.InputField>().text);
        level = Mathf.Clamp(level, 0, 29);
        //print(level);
        //gameObject.SetActive(false);
        acceptInput = false;
        canvas.enabled = false;
        game.SetActive(true);

        comp.startlevel = level;
        gameCanvas.SetActive(true);
        comp.Reset();
    }

    public void BackToMenu()
    {
        acceptInput = true;
        //gameObject.SetActive(true);
        canvas.enabled = true;
        comp.ClearBoard();
        //game.SetActive(false);
        gameCanvas.SetActive(false);
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
        //writer.Write("<SET ID=\"ENABLE_SEND_CURSOR\" STATE=\"1\" />\r\n");
        writer.Write("<SET ID=\"ENABLE_SEND_DATA\" STATE=\"1\" />\r\n");

        writer.Write("<SET ID=\"CALIBRATE_SHOW\" STATE=\"1\" />\r\n");
        writer.Flush();
        writer.Write("<SET ID=\"CALIBRATE_START\" STATE=\"1\" />\r\n");
        writer.Flush();
    }
}