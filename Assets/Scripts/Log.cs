﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class EyetrackLogline
{
    public string ETT;
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

public class Log : MonoBehaviour {

    List<string> data = new List<string>();
    string masterLog = "";
    string[] logHeaderArray = {"ts","event_type", "SID","ECID","session","game_type","game_number","episode_number","level","score","lines_cleared",
        "completed","game_duration","avg_ep_duration","zoid_sequence","evt_id","evt_data1","evt_data2",
        "curr_zoid","next_zoid","danger_mode",
        "evt_sequence","rots","trans","path_length",
        "min_rots","min_trans","min_path",
        "min_rots_diff","min_trans_diff","min_path_diff",
        "u_drops","s_drops","prop_u_drops",
        "initial_lat","drop_lat","avg_lat",
        "tetrises_game","tetrises_level",
         "agree","delaying","dropping","zoid_rot","zoid_col","zoid_row","board_rep","zoid_rep", "eye_tracker_time", "FPOGX", "FPOGY", "FPOGS", "FPOGD", "FPOGID", "FPOGV", "BPOGX", "BPOGY","BPOGV"};
    string[] gameStateList = { "SID","ECID","session","game_type","game_number","episode_number",
    "level","score","lines_cleared","danger_mode",
    "delaying","dropping","curr_zoid","next_zoid",
    "zoid_rot","zoid_col","zoid_row","board_rep","zoid_rep" };
    string[] episodeList = { "SID","ECID","session","game_type","game_number","episode_number",
      "level","score","lines_cleared",
      "curr_zoid","next_zoid","danger_mode",
      "zoid_rot","zoid_col","zoid_row",
      "board_rep","zoid_rep","evt_sequence","rots","trans","path_length",
      "min_rots","min_trans","min_path",
      "min_rots_diff","min_trans_diff","min_path_diff",
      "u_drops","s_drops","prop_u_drops",
      "initial_lat","drop_lat","avg_lat",
      "tetrises_game","tetrises_level",
      "agree" };
    string[] eventList = { "SID","ECID","session","game_type","game_number","episode_number",
      "level","score","lines_cleared",
      "curr_zoid","next_zoid","danger_mode",
      "delaying","dropping",
      "zoid_rot","zoid_col","zoid_row" };
    string[] summList = { "SID","ECID","session","game_type","game_number","episode_number",
    "level","score","lines_cleared","completed",
    "game_duration","avg_ep_duration","zoid_sequence" };
    string[] eyeList = { "eye_tracker_time", "FPOGX", "FPOGY", "FPOGS", "FPOGD", "FPOGID", "FPOGV", "BPOGX", "BPOGY", "BPOGV" };
    public string logHeader;
    string[] loglist;
    public Game game;
    public string eyetrackString= "<REC TIME=\"38.41262\" FPOGX=\"0.41352\" FPOGY=\"0.80682\" FPOGS=\"38.36427\" FPOGD=\"0.04835\" FPOGID=\"93\" FPOGV=\"1\" BPOGX=\"0.43701\" BPOGY=\"0.81848\" BPOGV=\"1\" />";
    EyetrackLogline eyetrackingLogline = new EyetrackLogline();
    char[] separatingChars = { ' ', '=', '"' };

    public void Awake(){
        logHeader = string.Join("\t", logHeaderArray);

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
        data.Add(eventType);
        logit(Settings.ECID.ToString(), "ECID");
        logit(Settings.session, "session");
        logit(Settings.gameType, "game_type");
        logit(game.gameNumber.ToString(), "game_number");
        logit(game.episode.ToString(), "episode_number");
        logit(game.level.ToString(), "level");
        logit(game.score.ToString(), "score");
        logit(game.lines.ToString(), "lines_cleared");
        logit(complete.ToString(), "completed");
        logit((Time.time - game.gameStartTime).ToString(), "game_duration");
        logit(((Time.time - game.gameStartTime) / (game.episode + 1)).ToString(), "avg_ep_duration");
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
        logit(Zoid.names[game.next].ToString(), "next_zoid");

        // Implement slice to skip first three rows??
        logit(PrettyPrint2DArray(game.board.contents), "board_rep");
        logit(PrettyPrint2DArray(game.zoid.ZoidRep()), "zoid_rep");

        logit(eyetrackingLogline.ETT, "eye_tracker_time");
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
        // TODO: process eyetrackingLogline from eyetrackString;
        // eyetrackString looks like:
        // <REC TIME="38.41262" FPOGX="0.41352" FPOGY="0.80682" FPOGS="38.36427" FPOGD="0.04835" FPOGID="93" FPOGV="1" BPOGX="0.43701" BPOGY="0.81848" BPOGV="1" />
        string[] eye = eyetrackString.Split(separatingChars);
        if (eye[0] == "<REC")
        {
            eyetrackingLogline.ETT = eye[3];
            eyetrackingLogline.FPOGX = eye[7];
            eyetrackingLogline.FPOGY = eye[11];
            eyetrackingLogline.FPOGS = eye[15];
            eyetrackingLogline.FPOGD = eye[19];
            eyetrackingLogline.FPOGID = eye[23];
            eyetrackingLogline.FPOGV = eye[27];
            eyetrackingLogline.BPOGX = eye[31];
            eyetrackingLogline.BPOGY = eye[35];
            eyetrackingLogline.BPOGV = eye[39];

            loglist = eyeList;
            LogUniversal("EYETRACK");
        }
    }

}
