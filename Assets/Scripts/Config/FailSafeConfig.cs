using System;
using UnityEngine;


[Serializable]
public class FailSafe
{
    public int loseStreakTrigger;
    public int damageMultiplier;
    public int hpMultiplier;
    public float rewardBonus;
}
[Serializable]
public class FailSafeConfig
{
    public FailSafe failSafe;
    private static FailSafeConfig _instance;
    public static FailSafeConfig Instance
    {
        get
        {
            if (_instance == null)
            {
                TextAsset json = Resources.Load<TextAsset>("Config/failsafe_config");
                if (json == null)
                {
                    Debug.LogError("❌ Missing failsafe_config.json");
                    _instance = new FailSafeConfig();
                }
                else
                {
                    _instance = JsonUtility.FromJson<FailSafeConfig>(json.text);
                }
            }
            return _instance;
        }
    }
}
