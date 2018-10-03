using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum InputType {keyboard, converted, retropad };


//todo: make canvas for lvl

public class SteadyCanvasScript : MonoBehaviour {

    public GameObject uiController;
    public UnityEngine.UI.Text textScore;

    UIControllerScript uiContrl;





	// Use this for initialization
	void Start () {
        uiContrl = uiController.GetComponent<UIControllerScript>();
    }
	
	// Update is called once per frame
	void Update () {

        if (Input.GetKeyDown(KeyCode.Return))
        {
            uiContrl.StartGame(InputType.keyboard);
        }
        else if (Input.GetKeyDown("joystick button 0"))
        {
            uiContrl.StartGame(InputType.converted);
        }
        else if (Input.GetKeyDown("joystick button 1"))
        {
            uiContrl.StartGame(InputType.retropad);
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

}
