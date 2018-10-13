using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class UIControllerScript : MonoBehaviour
{
    public GameObject readyCanvas;
    public GameObject steadyCanvas;
    public GameObject gameCanvas;

    public GameObject line;
    public GameObject nextLine;
    public GameObject game;

    public GameObject eyeTracker;

    Game gameScript;
    SteadyCanvasScript steadyScript;
    EyeTrackerScript eyeScript;

    // Use this for initialization
    void Start()
    {
        readyCanvas.SetActive(true);
        steadyCanvas.SetActive(false);
        gameCanvas.SetActive(false);
        line.SetActive(false);
        nextLine.SetActive(false);

        gameScript = game.GetComponent<Game>();
        steadyScript = steadyCanvas.GetComponent<SteadyCanvasScript>();
        eyeScript = eyeTracker.GetComponent<EyeTrackerScript>();
    }


    public void BeginExperiment(string sid, string ecid, Dropdown gameType, int inType)
    {
        if (!string.IsNullOrEmpty(sid) && !string.IsNullOrEmpty(ecid))
        {
            Settings.subjectID = sid;
            Settings.ECID = ecid;

            Settings.inpt = (InputType)inType;

            Settings.gameType = gameType.options[gameType.value].text;
            Settings.sessionTime = 0;
            switch (gameType.value)
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

            steadyScript.AdjustLayout(gameType.value);
            readyCanvas.SetActive(false);
            steadyCanvas.SetActive(true);
            eyeScript.startNewLog();
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
        int score = gameScript.score;

        long tick;
        Log.QueryPerformanceCounter(out tick);
        eyeScript.ketchUp(tick);

        gameScript.ClearBoard();
        game.SetActive(false);

        gameCanvas.SetActive(false);
        line.SetActive(false);
        nextLine.SetActive(false);

        steadyCanvas.SetActive(true);
        steadyScript.SetScore(score);
    }

    public void FinishExperiment()
    {
        steadyCanvas.SetActive(false);
        readyCanvas.SetActive(true);
    }

    public void SetStartLvl(Dropdown drop)
    {
        Settings.startLevel = drop.value;
    }

}
