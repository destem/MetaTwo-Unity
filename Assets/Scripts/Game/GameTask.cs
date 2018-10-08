using System.Collections;
using System.Collections.Generic;
using UnityEngine;



/* TODO: Will extend this class in future to manage different game settings,
        along with an ExpSettings class for different exp. setups.
 */
public class GameTask : MonoBehaviour
{

    Game game;

    bool active = false;

    void Start()
    {
        game = GetComponent<Game>();
    }

    public void StartTask()
    {
        active = true;
    }



    void Update()
    {
        if (active && Settings.sessionTime > 0)
        {
            if (Settings.sessionTime < Time.time - Settings.startTime)
            {
                game.GameOver();
                active = false;
            }
        }

    }

}

