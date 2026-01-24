using UnityEngine;

public class SupporterManager : MonoBehaviour
{
    public static SupporterManager Instance;

    public BossData currentSupporter;

    void Awake()
    {
        Instance = this;
    }

    public void Equip(BossData boss)
    {
        currentSupporter = boss;
        ApplyBuff();
    }

    void ApplyBuff()
    {
        float atkBonus = bossAtk() / 2f;
        float hpBonus = bossHp() / 2f;

        foreach (var unit in BattleManager.Instance.playerTeam)
        {
            MonsterHealth mh = unit.GetComponent<MonsterHealth>();
            mh.stats.attackDamage += (int)atkBonus;
            mh.stats.maxHP += (int)hpBonus;
            mh.stats.currentHP = mh.stats.maxHP;
        }
    }

    float bossAtk() => currentSupporter.tier * 50f;
    float bossHp() => currentSupporter.tier * 200f;
}
