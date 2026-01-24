using UnityEngine;
public enum BossSkillType
{
    FanShot,        // bắn quạt
    PiercingCircle  // đạn tròn xuyên
}

[CreateAssetMenu(menuName = "Boss/BossData")]
public class BossData : ScriptableObject
{
    public int bossId;
    public string bossName;

    [Header("Stats")]
    public int tier;              // tier 3, tier 4...
    public float hpMultiplier = 3f;
    public float sizeScale = 1.5f;

    [Header("Reward")]
    public int gemReward;
    public long coinReward;

    [Header("Skill")]
    public BossSkillType skillType;
    public int triggerAfterHits; // n đòn đánh
}
