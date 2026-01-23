using System;
using UnityEngine;

[Serializable]
public class EconomyConfig
{
    public UnitShopConfig unitShop;

    private static EconomyConfig _instance;
    public static EconomyConfig Instance
    {
        get
        {
            if (_instance == null)
            {
                TextAsset json = Resources.Load<TextAsset>("Config/economy_config");
                if (json == null)
                {
                    Debug.LogError("❌ Missing economy_config.json");
                    _instance = new EconomyConfig();
                }
                else
                {
                    _instance = JsonUtility.FromJson<EconomyConfig>(json.text);
                }
            }
            return _instance;
        }
    }
}

[Serializable]
public class UnitShopConfig
{
    public StartCostConfig startCost;
    public float costIncreaseRate;
    public int increaseAfterLevel;
}

[Serializable]
public class StartCostConfig
{
    public long melee;
    public long range;
}
