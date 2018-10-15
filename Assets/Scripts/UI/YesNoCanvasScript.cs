using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class YesNoCanvasScript : MonoBehaviour {

    public Text msg;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}


    public void setMsg(string newMsg)
    {
        this.msg.text = newMsg;

    }
}
