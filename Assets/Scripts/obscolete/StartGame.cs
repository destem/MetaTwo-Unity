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
    public GameObject line;
    public GameObject nextLine;
    public GameObject game;
    public GameObject eyeTracker;
    public GameObject steadyCanvas;

    Game comp;
    EyeTrackerScript eyeTracker_Script;
    Canvas canvas;
    bool acceptInput = false;

    TcpClient socket;
    NetworkStream stream;
    StreamWriter writer;
    StreamReader reader;
    bool socketReady = false;
    char[] buffer = new char[4096];

    GameObject inputSID;
    GameObject inputECID;
    GameObject inputGameType;
    GameObject inputLevel;

    // Use this for initialization
    void Start()
    {
        inputLevel = GameObject.Find("Input_Level");
        inputSID = GameObject.Find("Input_SID");
        inputECID = GameObject.Find("Input_ECID");
        inputGameType = GameObject.Find("Input_GameType");

        eyeTracker_Script = eyeTracker.GetComponent<EyeTrackerScript>();
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
          
            comp.log.eyetrackString = reader.ReadLine();
            comp.log.LogEyeTracker();

            // clear read buffer. Eyetracker is running at roughly 60Hz, but just in case
            // NOTE: doing this keeps it in sync but misses every other frame, leading to one eyetrack event for every two game_state loglines
            //reader.Read(buffer, 0, buffer.Length);
           
        }
    }

 

    public void LaunchGame()
    {
        Settings.subjectID = inputSID.GetComponent<UnityEngine.UI.InputField>().text;
        Settings.ECID = inputECID.GetComponent<UnityEngine.UI.InputField>().text;
        Settings.gameType = inputGameType.GetComponent<UnityEngine.UI.InputField>().text;

        int level = System.Int32.Parse(inputLevel.GetComponent<UnityEngine.UI.InputField>().text);


        level = Mathf.Clamp(level, 0, 29);
        //print(level);
        //gameObject.SetActive(false);
        acceptInput = false;
        canvas.enabled = false;

        game.SetActive(true);
        line.SetActive(true);
        nextLine.SetActive(true);

        comp.startlevel = level;
        gameCanvas.SetActive(true);
        comp.Reset();

        //todo: added
        steadyCanvas.SetActive(false);
    }

    public void BackToMenu()
    {
        
        //gameObject.SetActive(true);
        canvas.enabled = true;
        steadyCanvas.SetActive(false);
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

    public void GotoSteady()
    {
        steadyCanvas.SetActive(true);

        if (comp.board != null)
        {
            comp.ClearBoard();
        }

      //game.SetActive(false);
        gameCanvas.SetActive(false);
        line.SetActive(false);
        nextLine.SetActive(false);

        acceptInput = true;
        canvas.enabled = false;
    }



}