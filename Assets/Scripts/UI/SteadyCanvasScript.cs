using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



//todo: make canvas for lvl

public class SteadyCanvasScript : MonoBehaviour
{

    public GameObject uiController;
    public Text textScore;
    public Text textStart;
    public Text textControls;
    public Text textStartLvl;
    public Dropdown dropDown_startLvl;

    UIControllerScript uiContrl;


    // Use this for initialization
    void Start()
    {
        uiContrl = uiController.GetComponent<UIControllerScript>();

        for (int i = 1; i < 19; i++)
        {
            dropDown_startLvl.options.Add(new Dropdown.OptionData { text = i.ToString() });
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (
           (Settings.inpt == InputType.keyboard && Input.GetKeyDown(KeyCode.Return))
        || (Settings.inpt == InputType.converted && Input.GetKeyDown("joystick button 0"))
        || (Settings.inpt == InputType.retropad && Input.GetKeyDown("joystick button 1"))
        )
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


    public void AdjustLayout(int gameType)
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

        bool chooseLvl = (gameType == 3);
        dropDown_startLvl.gameObject.SetActive(chooseLvl);
        textStartLvl.enabled = chooseLvl;
    }

}
