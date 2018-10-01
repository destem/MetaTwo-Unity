using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class UIControllerScript : MonoBehaviour
{
    public GameObject readyCanvas;
    public GameObject steadyCanvas;
    public GameObject gameCanvas;

    public GameObject line;
    public GameObject nextLine;
    public GameObject game;

    public GameObject eyeTrack;

    EyeTrackerScript eyeTrackerScript;
    Game gameScript;
    int startLevel;



    // Use this for initialization
    void Start()
    {
        readyCanvas.SetActive(true);
        steadyCanvas.SetActive(false);
        gameCanvas.SetActive(false);

        gameScript = game.GetComponent<Game>();
        eyeTrackerScript = eyeTrack.GetComponent<EyeTrackerScript>();
    }




    public void BeginExperiment(string sid, string ecid, string gameType, string lvl)
    {

        if (!string.IsNullOrEmpty(sid) && 
            !string.IsNullOrEmpty(ecid) && 
            !string.IsNullOrEmpty(gameType) && 
            !string.IsNullOrEmpty(lvl))
        {
            Settings.subjectID = sid;
            Settings.ECID = ecid;
            Settings.gameType = gameType;

            startLevel = System.Int32.Parse(lvl);
            startLevel = Mathf.Clamp(startLevel, 0, 29);

            readyCanvas.SetActive(false);
            steadyCanvas.SetActive(true);
        }
    }


    public void StartGame(InputType inputType)
    {

        switch (inputType)
        {
            case InputType.keyboard:

                gameScript.useKeyboard = true;
                gameScript.useTomee = false;
                gameScript.useRetro = false;
                break;

            case InputType.converted:

                gameScript.useKeyboard = false;
                gameScript.useTomee = true;
                gameScript.useRetro = false;
                break;

            case InputType.retropad:

                gameScript.useKeyboard = false;
                gameScript.useTomee = false;
                gameScript.useRetro = true;
                break;
        }

        steadyCanvas.SetActive(false);

        game.SetActive(true);
        line.SetActive(true);
        nextLine.SetActive(true);

        gameScript.startlevel = startLevel;
        gameCanvas.SetActive(true);
        gameScript.Reset();
    }


    public void FinishGame()
    {
        int score = gameScript.score;

        gameScript.ClearBoard();

        gameCanvas.SetActive(false);
        line.SetActive(false);
        nextLine.SetActive(false);

        steadyCanvas.SetActive(true);
        steadyCanvas.GetComponent<SteadyCanvasScript>().SetScore(score);
    }

    public void FinishExperiment()
    {
        steadyCanvas.SetActive(false);
        readyCanvas.SetActive(true);
    }

}
