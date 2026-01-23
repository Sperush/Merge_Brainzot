public static class LoseTracker
{
    public static int loseStreak;

    public static void OnWin()
    {
        loseStreak = 0;
    }

    public static void OnLose()
    {
        loseStreak++;
        if (IsFailSafeActive())
        {
            GameLog.Log("failsafe_active", new
            {
                loseStreak = loseStreak
            });
        }
    }
    public static bool IsFailSafeActive()
    {
        return loseStreak >= FailSafeConfig.Instance.failSafe.loseStreakTrigger;
    }
}