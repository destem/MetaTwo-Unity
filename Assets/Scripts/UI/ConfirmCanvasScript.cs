using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmCanvasScript : MonoBehaviour {


    GameObject returnCanvas;
    public GameObject UiController;

    UIControllerScript uiContrl;

    public Text msg;

	void Start () {
        uiContrl = UiController.GetComponent<UIControllerScript>();
	}
	
	// Update is called once per frame
	void Update () {
		
	}


    public void NewMessage (string newMsg, GameObject returnCanv){
        this.msg.text = newMsg;
        this.returnCanvas = returnCanv;
    }

    public void ButtonOkClick()
    {
        uiContrl.GoTo(returnCanvas);
    }
}
