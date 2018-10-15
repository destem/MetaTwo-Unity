using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class SteadyCanvasScript : MonoBehaviour
{

    public GameObject uiController;
    public Text textStart;
    public Text textControls;
    public Text textStartLvl;
    public Dropdown dropDown_startLvl;
    public Dropdown dropDown_gameTask;

    public GameObject panel_startLvl;

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

    public void ShowLvl(Dropdown gametask)
    {
        bool chooseLvl = (gametask.value == 3);
        panel_startLvl.SetActive(chooseLvl);
        dropDown_startLvl.value = 0;
    }


    public void AdjustInput(InputType inpt)
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

    public void ResetCanvasLayout()
    {
        dropDown_gameTask.value = 0;
        ShowLvl(dropDown_startLvl);

    }

}
