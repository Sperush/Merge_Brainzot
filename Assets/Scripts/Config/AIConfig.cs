using System;
using UnityEngine;

[Serializable]
public class AIConfig
{
    public MeleeAIConfig melee;
    public RangeAIConfig range;
    public TargetingConfig targeting;

    private static AIConfig _instance;
    public static AIConfig Instance
    {
        get
        {
            if (_instance == null)
            {
                TextAsset json = Resources.Load<TextAsset>("Config/ai_config");
                if (json == null)
                {
                    Debug.LogError("❌ Missing ai_config.json");
                    _instance = new AIConfig();
                }
                else
                {
                    _instance = JsonUtility.FromJson<AIConfig>(json.text);
                }
            }
            return _instance;
        }
    }
}

[Serializable]
public class MeleeAIConfig
{
    public float laneTolerance;
    public float xTolerance;
}

[Serializable]
public class RangeAIConfig
{
    public float laneTolerance;
    public float projectileForce;
}

[Serializable]
public class TargetingConfig
{
    public float searchRadius;
}
