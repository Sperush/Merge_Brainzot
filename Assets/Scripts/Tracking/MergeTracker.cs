using UnityEngine;

public static class MergeTracker
{
    public static int mergeCount;
    public static float timeToFirstMerge = -1f;

    public static void OnMerge()
    {
        mergeCount++;

        if (timeToFirstMerge < 0)
            timeToFirstMerge = Time.timeSinceLevelLoad;
    }

    public static void Reset()
    {
        mergeCount = 0;
        timeToFirstMerge = -1f;
    }
}