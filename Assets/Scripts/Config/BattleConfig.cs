using System;
using UnityEngine;

[Serializable]
public class BattleConfig
{
    public RewardConfig reward;
    public DifficultyConfig difficulty;

    private static BattleConfig _instance;
    public static BattleConfig Instance
    {
        get
        {
            if (_instance == null)
            {
                TextAsset json = Resources.Load<TextAsset>("Config/battle_config");
                if (json == null)
                {
                    Debug.LogError("❌ Missing battle_config.json");
                    _instance = new BattleConfig();
                }
                else
                {
                    _instance = JsonUtility.FromJson<BattleConfig>(json.text);
                }
            }
            return _instance;
        }
    }
}

[Serializable]
public class RewardConfig
{
    public float winMultiplier;
    public float loseMultiplier;
}

[Serializable]
public class DifficultyConfig
{
    public SerializableDictionary<int, float> manualLevels;
    public float[] baseGroup;
    public float groupIncrease;
}
