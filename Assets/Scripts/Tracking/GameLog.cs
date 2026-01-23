using UnityEngine;

public static class GameLog
{
    public static void Log(string evt, object data = null)
    {
        Debug.Log($"[LOG] {evt} :: {JsonUtility.ToJson(data)}");
    }
    // Sau này thay bằng Firebase / Adjust
    //public static void Log(string evt, Dictionary<string, object> data = null)
    //{
    //    Firebase.Analytics.FirebaseAnalytics.LogEvent(evt, data);
    //}
}
