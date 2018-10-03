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

    Game gameScript;
    int startLevel=9;


    string keyboardMsg = "Press 'Enter' \n to Start";

    // Use this for initialization
    void Start()
    {
        readyCanvas.SetActive(true);
        steadyCanvas.SetActive(false);
        gameCanvas.SetActive(false);

        gameScript = game.GetComponent<Game>();
    }




    public void BeginExperiment(string sid, string ecid, int gameType, int inputType)
    {

        if (!string.IsNullOrEmpty(sid) && !string.IsNullOrEmpty(ecid))
        {
            Settings.subjectID = sid;
            Settings.ECID = ecid;

            switch (gameType)
            {
                case 0:
                    Settings.gameType = "Key Mashing";
                    break;
                case 1:
                    Settings.gameType = "9 till Die";
                    break;
                case 2:
                    Settings.gameType = "0 till Die";
                    break;
                case 3:
                    Settings.gameType = "pick till Die";
                    break;
            }


            switch (inputType)
            {
                case 0:
                    gameScript.useKeyboard = true;
                    gameScript.useTomee = false;
                    gameScript.useRetro = false;
                    break;
                case 1:
                    gameScript.useKeyboard = false;
                    gameScript.useTomee = true;
                    gameScript.useRetro = false;
                    break;
                case 2:
                    gameScript.useKeyboard = false;
                    gameScript.useTomee = false;
                    gameScript.useRetro = true;
                    break;
            }


            //todo: do lvl
            //startLevel = System.Int32.Parse(lvl);
            //startLevel = Mathf.Clamp(startLevel, 0, 29);

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
        game.SetActive(false);

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
