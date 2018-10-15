using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public delegate void messageDeletage();


public class MessageCanvasScript : MonoBehaviour {


    GameObject okCanvas;
    GameObject cancelCanvas;

    messageDeletage executeOnOk;

    public GameObject UiController;

    public GameObject okButton;
    public GameObject okCancelPanel;

    UIControllerScript uiContrl;

    public Text msg;

	void Start () {
        uiContrl = UiController.GetComponent<UIControllerScript>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}


    public void NewMessage(string newMsg, GameObject newOkCanvas)
    {
        this.msg.text = newMsg;
        this.okCanvas = newOkCanvas;
        this.okButton.SetActive(true);
        this.okCancelPanel.SetActive(false);
    }


    public void NewMessage(string newMsg, GameObject newOkCanvas, GameObject newCancelCanvas)
    {
        this.msg.text = newMsg;
        this.okCanvas = newOkCanvas;
        this.cancelCanvas = newCancelCanvas;
        this.okButton.SetActive(false);
        this.okCancelPanel.SetActive(true);
    }

    public void NewMessage(string newMsg, GameObject newOkCanvas, GameObject newCancelCanvas, messageDeletage newTask) 
    {
        this.NewMessage(newMsg, newOkCanvas, newCancelCanvas);
        this.executeOnOk = newTask;
    }


    public void ButtonOkClick()
    {
        uiContrl.GoTo(okCanvas);

        if(this.executeOnOk!= null)
        {
            this.executeOnOk();
            this.executeOnOk = null;
        }
    }


    public void ButtonCancelClick()
    {
        uiContrl.GoTo(cancelCanvas);
    }


}
