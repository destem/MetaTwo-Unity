using System.Collections;
using System.Collections.Generic;
using UnityEngine;



//todo: make canvas for lvl

public class SteadyCanvasScript : MonoBehaviour
{

    public GameObject uiController;
    public UnityEngine.UI.Text textScore;
    public UnityEngine.UI.Text textStart;
    public UnityEngine.UI.Text textControls;

    UIControllerScript uiContrl;


    // Use this for initialization
    void Start()
    {
        uiContrl = uiController.GetComponent<UIControllerScript>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Settings.inpt == InputType.keyboard && Input.GetKeyDown(KeyCode.Return))
        {
            uiContrl.StartGame();
        }
        else if (Input.GetKeyDown("joystick button 0") || Input.GetKeyDown("joystick button 1"))
        {
            uiContrl.StartGame();
        }

    }


    public void ButtonFinishExpClick()
    {
        uiContrl.FinishExperiment();
        textScore.text = "";
    }

    public void SetScore(int score)
    {
        textScore.text = "Last Score: " + score;
    }


    public void UpdateCaptions()
    {
        if (Settings.inpt == InputType.keyboard)
        {
            textStart.text = "Press 'Enter'\nto Start";
            textControls.enabled = true;
        }
        else
        {
            textStart.text = "Press 'A' Button\nto Start";
            textControls.enabled = false;
        }

    }
}
