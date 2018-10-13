using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Runtime.InteropServices;
using System.IO;

public class EyetrackLogline
{
    public string ETT;
    public string TIMETICK;
    public string FPOGX;
    public string FPOGY;
    public string FPOGS;
    public string FPOGD;
    public string FPOGID;
    public string FPOGV;
    public string BPOGX;
    public string BPOGY;
    public string BPOGV;
}

public class Log : MonoBehaviour
{

    List<string> data = new List<string>();
    List<char> nextZoids = new List<char>();
    string masterLog = "";
    string[] logHeaderArray = {"ts","system_ticks", "event_type", "SID","ECID","session","game_type","game_number","episode_number","level","score","lines_cleared",
        "completed","game_duration","avg_ep_duration","zoid_sequence","evt_id","evt_data1","evt_data2",
        "curr_zoid","next_zoid","board_rep","zoid_rep"};
    string[] gameStateList = { "SID","ECID","session","game_type","game_number","episode_number",
    "level","score","lines_cleared","curr_zoid", "next_zoid", "board_rep","zoid_rep" };
    string[] episodeList = { "SID","ECID","session","game_type","game_number","episode_number",
      "level","score","lines_cleared",
      "curr_zoid","next_zoid"};
    string[] eventList = { "SID","ECID","session","game_type","game_number","episode_number",
      "level","score","lines_cleared",
      "curr_zoid","next_zoid" };
    string[] summList = { "SID","ECID","session","game_type","game_number","episode_number",
    "level","score","lines_cleared","completed",
    "game_duration","avg_ep_duration","zoid_sequence" };
    string[] eyeList = { "eye_tracker_time", "eye_time_tick", "FPOGX", "FPOGY", "FPOGS", "FPOGD", "FPOGID", "FPOGV", "BPOGX", "BPOGY", "BPOGV" };
    public string logHeader;
    string[] loglist;
    public Game game;
    public string eyetrackString = "";
    EyetrackLogline eyetrackingLogline = new EyetrackLogline();
    char[] separatingChars = { ' ', '=', '"' };


    [DllImport("Kernel32.dll")]
    public static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

    [DllImport("Kernel32.dll")]
    public static extern bool QueryPerformanceFrequency(
        out long lpFrequency);



    public void Awake()
    {
        logHeader = string.Join("\t", logHeaderArray);

        string path;
        // on mac/windows builds, log folder is created besides the .exe / .app
        switch (Application.platform)
        {
            case RuntimePlatform.WindowsPlayer:
                path = Path.GetDirectoryName(Application.dataPath);
                // alternative: path += "/..";
                break;

            case RuntimePlatform.OSXPlayer:
                path = Path.GetDirectoryName(Path.GetDirectoryName(Application.dataPath));
                break;

            default:
                path = Application.persistentDataPath;
                break;
        }

        path += "/Logs";
        Directory.CreateDirectory(path);
        Settings.logDir = path;
    }


    string PrettyPrint2DArray(int[,] intArray)
    {
        string outString = "";
        List<string> tempList = new List<string>();
        List<string> masterList = new List<string>();
        int rowLength = intArray.GetLength(0);
        int colLength = intArray.GetLength(1);

        // if we're printing the board, we don't want to include the three invisible spaces at the top
        for (int i = rowLength == 23 ? 3 : 0; i < rowLength; i++)
        {
            for (int j = 0; j < colLength; j++)
            {
                //outString += string.Format("{0} ", intArray[i, j]);
                tempList.Add(intArray[i, j].ToString());
            }
            masterList.Add("[" + string.Join(",", tempList.ToArray()) + "]");
            tempList.Clear();
        }
        outString = "[" + string.Join(",", masterList.ToArray()) + "]";
        return (outString);
    }


    public void LogExpHeader()
    {
        game.writer.WriteLine("Meta-Two build\t" + System.IO.File.GetLastWriteTime(System.Reflection.Assembly.GetExecutingAssembly().Location));
        game.writer.WriteLine("Game Start\t" + System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
        long tick;
        QueryPerformanceCounter(out tick);
        game.writer.WriteLine("GameStart Tick\t" + tick);
        QueryPerformanceFrequency(out tick);
        game.writer.WriteLine("CPU Tick Frequency\t" + tick);
        game.writer.WriteLine("SID\t" + Settings.subjectID);
        game.writer.WriteLine("ECID\t" + Settings.ECID);
        game.writer.WriteLine("GameTask\t" + Settings.gameType);
        game.writer.WriteLine("Session\t" + Settings.session);
        game.writer.WriteLine("GameNr.\t" + Settings.gameNumber);
        game.writer.WriteLine("Input type\t" + Settings.inpt);
        game.writer.WriteLine("randSeed\t" + Settings.randomSeed);
        game.writer.WriteLine("Screen resolution\t" + Screen.currentResolution);
        game.writer.WriteLine("Screen dpi\t" + Screen.dpi);
        game.writer.WriteLine("Fullscreen\t" + Screen.fullScreen.ToString());
        game.writer.WriteLine("Window height\t" + Screen.height);
        game.writer.WriteLine("Window width\t" + Screen.width);
        //game.writer.WriteLine("\t" +);
        //game.writer.WriteLine("\t" +);
    }

    void logit(string val, string key)
    {
        if (loglist.Contains(key))
        {
            data.Add(val);
        }
        else
        {
            data.Add("");
        }
    }

    void LogUniversal(string eventType, bool complete = false, string evtID = "", string evtData1 = "", string evtData2 = "")
    {
        data.Clear();
        data.Add(Time.time.ToString());

        long tick;
        QueryPerformanceCounter(out tick);
        data.Add(tick.ToString());

        data.Add(eventType);
        logit(Settings.subjectID, "SID");
        logit(Settings.ECID, "ECID");
        logit(Settings.session, "session");
        logit(Settings.gameType, "game_type");
        logit(Settings.gameNumber.ToString(), "game_number");
        logit(game.episode.ToString(), "episode_number");
        logit(game.level.ToString(), "level");
        logit(game.score.ToString(), "score");
        logit(game.lines.ToString(), "lines_cleared");
        logit(complete.ToString(), "completed");
        logit((Time.time - Settings.startTime).ToString(), "game_duration");
        logit(((Time.time - Settings.startTime) / (game.episode + 1)).ToString(), "avg_ep_duration");
        //comes in GAME_SUMM and looks like"
        // '["T", "Z", "O", "O", "T", "I", "T", "Z", "L", "I", "I", "O", "S", "Z", "T"]'
        logit("[" + string.Join(",", game.zoidBuff.ToArray()) + "]", "zoid_sequence");
        if (evtID != "")
        {
            data.Add(evtID);
        }
        else
        {
            data.Add("");
        }
        if (evtData1 != "")
        {
            data.Add(evtData1);
        }
        else
        {
            data.Add("");
        }
        if (evtData2 != "")
        {
            data.Add(evtData2);
        }
        else
        {
            data.Add("");
        }
        logit(Zoid.names[game.curr].ToString(), "curr_zoid");

        if (Settings.numPreviewZoids < 2)
        {
            logit(Zoid.names[game.next].ToString(), "next_zoid");
        }
        else
        {
            nextZoids.Clear();
            for (int i = 0; i < game.previewZoidQueue.Count; i++)
            {
                nextZoids.Add(Zoid.names[game.previewZoidQueue.ElementAt(i).zoidType]);
            }
            logit(new string(nextZoids.ToArray()), "next_zoid");
        }

        // Implement slice to skip first three rows??
        logit(PrettyPrint2DArray(game.board.contents), "board_rep");
        logit(PrettyPrint2DArray(game.zoid.ZoidRep()), "zoid_rep");

        logit(eyetrackingLogline.ETT, "eye_tracker_time");
        logit(eyetrackingLogline.TIMETICK, "eye_time_tick");
        logit(eyetrackingLogline.FPOGX, "FPOGX");
        logit(eyetrackingLogline.FPOGY, "FPOGY");
        logit(eyetrackingLogline.FPOGS, "FPOGS");
        logit(eyetrackingLogline.FPOGD, "FPOGD");
        logit(eyetrackingLogline.FPOGID, "FPOGID");
        logit(eyetrackingLogline.FPOGV, "FPOGV");
        logit(eyetrackingLogline.BPOGX, "BPOGX");
        logit(eyetrackingLogline.BPOGY, "BPOGY");
        logit(eyetrackingLogline.BPOGV, "BPOGV");

        //masterLog += string.Join("\t", data.ToArray());// + "\n";
        //print(masterLog);
        game.writer.WriteLine(string.Join("\t", data.ToArray()));

    }

    public void LogWorld()
    {
        loglist = gameStateList;
        LogUniversal("GAME_STATE");
        // not flushing here to save performance
    }

    public void LogEpisode()
    {
        loglist = episodeList;
        LogUniversal("EP_SUMM");
        game.writer.Flush();
    }

    public void LogGameSumm()
    {
        loglist = summList;
        LogUniversal("GAME_SUMM");
        game.writer.Close();
    }

    public void LogEvent(string id, string d1, string d2)
    {
        loglist = eventList;
        LogUniversal("GAME_EVENT", false, id, d1, d2);
        game.writer.Flush();
    }

    public void LogEyeTracker()
    {
        // eyetrackString looks like:
        // <REC TIME="38.41262" FPOGX="0.41352" FPOGY="0.80682" FPOGS="38.36427" FPOGD="0.04835" FPOGID="93" FPOGV="1" BPOGX="0.43701" BPOGY="0.81848" BPOGV="1" />
        string[] eye = eyetrackString.Split(separatingChars);
        if (eye[0] == "<REC")
        {
            eyetrackingLogline.ETT = eye[3];
            eyetrackingLogline.TIMETICK = eye[7];
            eyetrackingLogline.FPOGX = eye[11];
            eyetrackingLogline.FPOGY = eye[15];
            eyetrackingLogline.FPOGS = eye[19];
            eyetrackingLogline.FPOGD = eye[23];
            eyetrackingLogline.FPOGID = eye[27];
            eyetrackingLogline.FPOGV = eye[31];
            eyetrackingLogline.BPOGX = eye[35];
            eyetrackingLogline.BPOGY = eye[39];
            eyetrackingLogline.BPOGV = eye[43];

            loglist = eyeList;
            LogUniversal("EYETRACK");
        }
    }

}
