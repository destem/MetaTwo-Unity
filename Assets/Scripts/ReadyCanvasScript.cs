using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReadyCanvasScript : MonoBehaviour
{
    public GameObject uiController;

    GameObject inputLevel;
    GameObject inputSID;
    GameObject inputECID;
    GameObject inputGameType;

    UIControllerScript uiContrl;


    // Use this for initialization
    void Start()
    {
        //TODO: ask marc about better way.
        inputLevel = GameObject.Find("Input_Level");
        inputSID = GameObject.Find("Input_SID");
        inputECID = GameObject.Find("Input_ECID");
        inputGameType = GameObject.Find("Input_GameType");

        uiContrl = uiController.GetComponent<UIControllerScript>();
    }


    public void BeginButtonClick()
    {
        uiContrl.BeginExperiment(inputSID.GetComponent<UnityEngine.UI.InputField>().text,
                                inputECID.GetComponent<UnityEngine.UI.InputField>().text,
                                inputGameType.GetComponent<UnityEngine.UI.InputField>().text,
                                inputLevel.GetComponent<UnityEngine.UI.InputField>().text
                                );
    }
}
