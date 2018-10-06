using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class GameTask : MonoBehaviour {

    Game game;

    //how long the game should be played in seconds.
    // playtime = 0  ->  no time limit.
    int playTime;
    int startLevel;

    bool active = false;

	void Start () {
        game = GetComponent<Game>();	
	}
	
    public void StartTask (int newPlayTime, int newStartLevel)
    {
        this.playTime = newPlayTime;
        this.startLevel = newStartLevel;
        active = true;
        this.game.Reset();
    }



    void Update()
    {
        if (active)
        {

            if (playTime > 0)
            {
                if (playTime < Time.time - game.gameStartTime)
                {
                    game.GameOver();
                }
            }
        }
    }

}

