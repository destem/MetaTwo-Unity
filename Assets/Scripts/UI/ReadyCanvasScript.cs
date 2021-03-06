﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ReadyCanvasScript : MonoBehaviour
{
    public GameObject uiController;

    public InputField inputSID;
    public InputField inputECID;
    public Dropdown inputController;

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
        uiContrl.BeginExperiment(inputSID.text, inputECID.text,inputController.value);
    }

    public void Reset()
    {
        this.inputSID.text = "";
    }
}
