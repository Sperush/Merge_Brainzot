using System;
using UnityEngine;

[Serializable]
public class Ads
{
    public bool InterEnable;
    public int RewardMultiplier;
    public int MinGameplaySec;
}
[Serializable]
public class AdsConfig
{
    public Ads adsConfig;
    private static AdsConfig _instance;
    public static AdsConfig Instance
    {
        get
        {
            if (_instance == null)
            {
                TextAsset json = Resources.Load<TextAsset>("Config/ads_config");
                if (json == null)
                {
                    Debug.LogError("❌ Missing ads_config.json");
                    _instance = new AdsConfig();
                }
                else
                {
                    _instance = JsonUtility.FromJson<AdsConfig>(json.text);
                }
            }
            return _instance;
        }
    }
}

