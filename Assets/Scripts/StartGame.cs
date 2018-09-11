using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartGame : MonoBehaviour
{

    public GameObject gameCanvas;
    public GameObject game;
    Game comp;
    Canvas canvas;

    // Use this for initialization
    void Start()
    {
        comp = game.GetComponent<Game>();
        canvas = GetComponent<Canvas>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LaunchGame()
    {
        int level = System.Int32.Parse(transform.Find("InputField").gameObject.GetComponent<UnityEngine.UI.InputField>().text);
        level = Mathf.Clamp(level, 0, 29);
        //print(level);
        //gameObject.SetActive(false);
        canvas.enabled = false;
        game.SetActive(true);

        comp.startlevel = level;
        gameCanvas.SetActive(true);
        comp.Reset();
    }

    public void BackToMenu()
    {
        //gameObject.SetActive(true);
        canvas.enabled = true;
        comp.ClearBoard();
        //game.SetActive(false);
        gameCanvas.SetActive(false);
    }
}