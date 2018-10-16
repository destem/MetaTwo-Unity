using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;



public class UIControllerScript : MonoBehaviour
{
    public GameObject readyCanvas;
    public GameObject steadyCanvas;
    public GameObject gameCanvas;
    public GameObject messageCanvas;

    public GameObject line;
    public GameObject nextLine;
    public GameObject game;

    public GameObject eyeTracker;

    Game gameScript;
    SteadyCanvasScript steadyScript;
    MessageCanvasScript messageScript;
    EyeTrackerScript eyeScript;

    static readonly string eyeTracker_connected = "Successfully connected to EyeTracking Device.";
    static readonly string eyeTracker_noConnection = "EyeTracking Device not found. If you intend to collect eye-data," +
                                                        " open GazePoint Control Center and restart Meta Two.";

    static readonly string confirmExit = "Exit Study?";


    void Awake()
    {
        gameScript = game.GetComponent<Game>();
        steadyScript = steadyCanvas.GetComponent<SteadyCanvasScript>();
        messageScript = messageCanvas.GetComponent<MessageCanvasScript>();
        eyeScript = eyeTracker.GetComponent<EyeTrackerScript>();

        string path;
        // on mac/windows builds, log folder is created besides the .exe / .app
        switch (Application.platform)
        {
            case RuntimePlatform.WindowsPlayer:
                path = Path.GetDirectoryName(Application.dataPath);
                // alternative: path += "/..";
                break;

            case RuntimePlatform.OSXPlayer:
                path = Path.GetDirectoryName(Path.GetDirectoryName(Application.dataPath));
                break;

            default:
                path = Application.persistentDataPath;
                break;
        }

        path += "/Logs";
        Directory.CreateDirectory(path);
        Settings.logDir = path;
    }



    void Start()
    {
        string msg;

        try
        {
            eyeScript.Connect();
            msg = eyeTracker_connected;
        }
        catch (System.Net.Sockets.SocketException e)
        {
            msg = eyeTracker_noConnection;
            eyeTracker.SetActive(false);
        }

        GoTo(messageCanvas);
        messageScript.NewMessage(msg, readyCanvas);
    }


    public void BeginExperiment(string sid, string ecid, int inType)
    {
        if (!string.IsNullOrEmpty(sid) && !string.IsNullOrEmpty(ecid))
        {
            Settings.subjectID = sid;
            Settings.ECID = ecid;

            Settings.inpt = (InputType)inType;

            Settings.expStartTime = System.DateTime.Now;
            Settings.subjectDir = Settings.logDir + "/" + Settings.expStartTime.ToString("yyMMdd-HHmmss_") + Settings.subjectID;
            Directory.CreateDirectory(Settings.subjectDir);

            Settings.gameNumber = 1;

            GoTo(steadyCanvas);
            steadyScript.ResetCanvasLayout();
            steadyScript.AdjustInput(Settings.inpt);


            if (eyeTracker.activeSelf && eyeScript.IsConnected())
            {
                eyeScript.Calibrate(true);
                eyeScript.StartNewLog();
            }
        }
    }


    public void SetGameTask(Dropdown gameTask)
    {
        Settings.gameType = gameTask.options[gameTask.value].text;
        Settings.sessionTime = 0;
        switch (gameTask.value)
        {
            case 0:
                Settings.sessionTime = 120;
                Settings.startLevel = 0;
                break;
            case 1:
                Settings.startLevel = 9;
                break;
            case 2:
                Settings.startLevel = 0;
                break;
            case 3:
                Settings.startLevel = 0;
                break;
        }
    }


    public void StartGame()
    {
        steadyCanvas.SetActive(false);

        if (Settings.sessionTime > 0)
        {
            game.GetComponent<GameTask>().StartTask();
        }
        Settings.gameStartTime = Time.time;
        Settings.gameTask = steadyScript.dropDown_gameTask.options[steadyScript.dropDown_gameTask.value].text;

        game.SetActive(true);
        line.SetActive(true);
        nextLine.SetActive(true);

        gameCanvas.SetActive(true);

        //todo: make option of random seeds
        //fixed set of randomseed
        //int i = (Settings.randSeeds.Length + Settings.gameNumber - 1) % Settings.randSeeds.Length;
        //Settings.randomSeed = Settings.randSeeds[i];

        //random randseed, still done, for logging of initial seed
        Settings.randomSeed = (int)Mathf.Ceil(Random.value * 10000);
        Random.InitState(Settings.randomSeed);

        gameScript.Reset();
    }


    public void FinishGame()
    {
        GoTo(messageCanvas);
        messageScript.NewMessage("Score:\n" + gameScript.score + "\n\nLines cleared:\n" + gameScript.lines, steadyCanvas);

        long tick;
        Log.QueryPerformanceCounter(out tick);

        gameScript.ClearBoard();
        game.SetActive(false);

        Settings.gameNumber++;
    }


    public void SetStartLvl(Dropdown drop)
    {
        Settings.startLevel = drop.value;
    }


    public void AskExitStudy()
    {
        GoTo(messageCanvas);
        messageScript.NewMessage(confirmExit, readyCanvas, steadyCanvas, ExitStudy);
    }

    public void ExitStudy()
    {
        if (eyeTracker.activeSelf && eyeScript.HasActiveLog())
        {
            eyeScript.FinishLog();
        }
    }


    public void GoTo(GameObject to)
    {
        readyCanvas.SetActive(false);
        steadyCanvas.SetActive(false);
        gameCanvas.SetActive(false);
        messageCanvas.SetActive(false);

        line.SetActive(false);
        nextLine.SetActive(false);

        to.SetActive(true);
    }
}
