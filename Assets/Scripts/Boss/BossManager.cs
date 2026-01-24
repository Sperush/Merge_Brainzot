using System.Collections.Generic;
using UnityEngine;

public class BossManager : MonoBehaviour
{
    public static BossManager Instance;

    public List<BossData> bosses;
    public int currentBossUnlocked = 0;

    void Awake()
    {
        Instance = this;
    }

    public void StartBossFight(int bossId)
    {
        BossData data = bosses[bossId];

        BattleManager bm = BattleManager.Instance;

        // clear enemy thường
        foreach (var e in bm.enemyTeam)
            Destroy(e);
        bm.enemyTeam.Clear();

        // spawn boss
        GameObject bossObj = Instantiate(bm.meleeEnemyPrefab);
        MonsterHealth mh = bossObj.GetComponent<MonsterHealth>();

        ApplyBossStats(mh, data);

        bm.enemyTeam.Add(bossObj);
        bm.StartBattle();
    }

    void ApplyBossStats(MonsterHealth boss, BossData data)
    {
        // lấy stat melee + range tier X
        boss.stats.level = data.tier;

        boss.SetStats(data.tier);

        boss.stats.maxHP = (int)(boss.stats.maxHP * data.hpMultiplier);
        boss.stats.currentHP = boss.stats.maxHP;

        boss.transform.localScale *= data.sizeScale;

        boss.gameObject.AddComponent<BossSkillController>().Init(data);
    }
}
