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
    public GameObject confirmCanvas;
    public GameObject yesNoCanvas;

    public GameObject line;
    public GameObject nextLine;
    public GameObject game;

    public GameObject eyeTracker;

    Game gameScript;
    SteadyCanvasScript steadyScript;
    ConfirmCanvasScript confirmScript;
    YesNoCanvasScript yesNoScript;
    EyeTrackerScript eyeScript;

    static readonly string eyeTracker_connected = "Successfully connected to EyeTracking Device.";
    static readonly string eyeTracker_noConnection = "EyeTracking Device not found. If you intend to collect eye-data," +
                                                        " open GazePoint Control Center and restart Meta Two.";

    static readonly string confirmExit = "Exit Study?";


    void Awake()
    {
        gameScript = game.GetComponent<Game>();
        steadyScript = steadyCanvas.GetComponent<SteadyCanvasScript>();
        confirmScript = confirmCanvas.GetComponent<ConfirmCanvasScript>();
        eyeScript = eyeTracker.GetComponent<EyeTrackerScript>();
        yesNoScript = yesNoCanvas.GetComponent<YesNoCanvasScript>();

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

        if (eyeScript.Connect())
        {
            msg = eyeTracker_connected;
        }
        else
        {
            msg = eyeTracker_noConnection;
        }

        GoTo(confirmCanvas);
        confirmScript.NewMessage(msg, readyCanvas);
    }


    public void BeginExperiment(string sid, string ecid, int inType)
    {
        if (!string.IsNullOrEmpty(sid) && !string.IsNullOrEmpty(ecid))
        {
            Settings.subjectID = sid;
            Settings.ECID = ecid;

            Settings.inpt = (InputType)inType;

            GoTo(steadyCanvas);
            steadyScript.ResetCanvasLayout();
            steadyScript.AdjustInput(Settings.inpt);


            //todo:abort on error, fix it
            eyeScript.Calibrate(true);
            eyeScript.startNewLog();
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

        game.SetActive(true);
        line.SetActive(true);
        nextLine.SetActive(true);

        gameCanvas.SetActive(true);

        if (Settings.sessionTime > 0)
        {
            game.GetComponent<GameTask>().StartTask();
        }

        Settings.startTime = Time.time;

        gameScript.Reset();
    }


    public void FinishGame()
    {
        GoTo(confirmCanvas);
        confirmScript.NewMessage("Score:\n" + gameScript.score + "\n\nLines cleared:\n" + gameScript.lines,steadyCanvas);

        long tick;
        Log.QueryPerformanceCounter(out tick);

        //TODO: potential break of the game, if game restart too fast ... ketchup not done?
        eyeScript.ketchUp(tick);

        gameScript.ClearBoard();
        game.SetActive(false);
    }


    public void SetStartLvl(Dropdown drop)
    {
        Settings.startLevel = drop.value;
    }


    public void ExitStudy()
    {
        GoTo(yesNoCanvas);
        yesNoScript.setMsg(confirmExit);
    }


    public void GoTo(GameObject to)
    {
        readyCanvas.SetActive(false);
        steadyCanvas.SetActive(false);
        gameCanvas.SetActive(false);
        yesNoCanvas.SetActive(false);
        confirmCanvas.SetActive(false);

        line.SetActive(false);
        nextLine.SetActive(false);

        to.SetActive(true);
    }
}
