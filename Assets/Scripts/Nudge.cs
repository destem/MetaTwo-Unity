using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Nudge : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
    // this comment exists solely as a test to determine whether I've got git working back and forth between the CogWorks laptop and my (Marc's) Macbook

	// Update is called once per frame
	void Update () {
        if (Input.GetButtonDown("Right")){
            
            transform.Translate(.24f, 0f, 0f);
        }
        if (Input.GetButtonDown("Left"))
        {

            transform.Translate(-.24f, 0f, 0f);
        }
		
	}
}
