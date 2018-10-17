using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum InputType { converted, keyboard, retropad };


public static class Settings
{
    public static System.DateTime expStartTime;
    public static int numPreviewZoids = 1;
    public static string subjectID = "no_val";
    public static string ECID = "no_val";

    public static int startLevel = 0;
    public static string gameType = "standard";
    public static string gameTask = "";
    public static float gameStartTime;
    public static int sessionTime = 0; // total time, in seconds, for this experimental session. 1 hour = 3600 seconds
    public static int randomSeed = 0;

    public static int session = 1;
    public static int gameNumber = 1;

    public static InputType inpt = InputType.keyboard;

    public static string logDir;
    public static string subjectDir;

    public static readonly int[] randSeeds = { 1824, 1957, 1993, 2002, 2009, 2018 };
}

//TODO: work in progress, split settings
//public static class TaskSettings
//{
//    public static float startTime;
//    public static string subjectID = "no_val";
//    public static string ECID = "no_val";
//    public static string session = "";
//    public static int gameNumber = 0;
//    public static int sessionTime = 300; // total time, in seconds, for this experimental session. 1 hour = 3600 seconds

//    //todo:enum gametypes
//    public static string gameType = "standard";
//}


////TODO: work in progress, split settings
//public static class GameSettings
//{
//    public static int startLevel = 0;
//    public static int numPreviewZoids = 1;
//    public static int randomSeed = 0;
//    public static int seed = -1; // seed for RNG. -1 means use the current time
//    public static InputType inpt = InputType.keyboard;
//}

//TODO: gameStatistics



//second git test