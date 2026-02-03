using System;
using UnityEngine;

[Serializable]
public class UnitStatsConfig
{
    public UnitGroup units;

    private static UnitStatsConfig _instance;
    public static UnitStatsConfig Instance
    {
        get
        {
            if (_instance == null)
            {
                TextAsset json = Resources.Load<TextAsset>("Config/unit_stats_config");
                if (json == null)
                {
                    Debug.LogError("❌ Missing unit_stats_config.json");
                    _instance = new UnitStatsConfig();
                }
                else
                {
                    _instance = JsonUtility.FromJson<UnitStatsConfig>(json.text);
                }
            }
            return _instance;
        }
    }
}

[Serializable]
public class UnitGroup
{
    public UnitCurve melee;
    public UnitCurve range;
}

[Serializable]
public class UnitCurve
{
    public int[] damage;
    public int[] hp;
    public float[] attackSpeed;
    public float[] attackRange;
    public float[] moveSpeed;
}
