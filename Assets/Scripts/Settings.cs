using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Settings{

    public static int numPreviewZoids = 1;
    public static int randomSeed = 0;
    public static string subjectID = "no_val";
    public static string ECID = "no_val";
    public static string session = "";
    public static int startLevel = 0;
    public static string gameType = "standard";
    public static int sessionTime = 300; // total time, in seconds, for this experimental session. 1 hour = 3600 seconds
    public static int seed = -1; // seed for RNG. -1 means use the current time
}

//second git test