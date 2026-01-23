using UnityEngine;

[System.Serializable]
public class SpinReward
{
    public string id;        // coin, gem, unit, ...
    public GameObject image;
    public int amount;
    [Range(0f, 1f)]
    public float weight;     // tỉ lệ trúng (gian lận ở đây)
}
