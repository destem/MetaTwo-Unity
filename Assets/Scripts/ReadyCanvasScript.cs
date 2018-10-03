using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReadyCanvasScript : MonoBehaviour
{
    public GameObject uiController;

    public GameObject inputLevel;
    public GameObject inputSID;
    public GameObject inputECID;
    public GameObject inputGameType;
    public GameObject inputController;

    UIControllerScript uiContrl;


    // Use this for initialization
    void Start()
    {
        uiContrl = uiController.GetComponent<UIControllerScript>();
    }


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    public void BeginButtonClick()
    {
        uiContrl.BeginExperiment(inputSID.GetComponent<UnityEngine.UI.InputField>().text,
                                inputECID.GetComponent<UnityEngine.UI.InputField>().text,
                                inputGameType.GetComponent<UnityEngine.UI.Dropdown>().value,
                                 inputController.GetComponent<UnityEngine.UI.Dropdown>().value
                                );
    }
}
