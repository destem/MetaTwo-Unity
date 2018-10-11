using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum InputType { keyboard, converted, retropad };


public static class Settings
{
    public static int numPreviewZoids = 3;
    public static int randomSeed = 0;
    public static string subjectID = "no_val";
    public static string ECID = "no_val";
    public static int startLevel = 0;
    public static string gameType = "standard";
    public static float startTime;
    public static int sessionTime = 0; // total time, in seconds, for this experimental session. 1 hour = 3600 seconds
    public static int seed = -1; // seed for RNG. -1 means use the current time

    public static string session = "";
    public static int gameNumber = 1;

    public static InputType inpt = InputType.keyboard;

}

//TODO: work in progress, split settings
public static class TaskSettings
{
    public static float startTime;
    public static string subjectID = "no_val";
    public static string ECID = "no_val";
    public static string session = "";
    public static int gameNumber = 0;
    public static int sessionTime = 300; // total time, in seconds, for this experimental session. 1 hour = 3600 seconds

    //todo:enum gametypes
    public static string gameType = "standard";
}


//TODO: work in progress, split settings
public static class GameSettings
{
    public static int startLevel = 0;
    public static int numPreviewZoids = 1;
    public static int randomSeed = 0;
    public static int seed = -1; // seed for RNG. -1 means use the current time
    public static InputType inpt = InputType.keyboard;
}

//TODO: gameStatistics



//second git test