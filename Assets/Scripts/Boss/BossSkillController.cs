using UnityEngine;

public class BossSkillController : MonoBehaviour
{
    private BossData data;
    private MonsterAI ai;
    private int hitCounter;

    public void Init(BossData bossData)
    {
        data = bossData;
        ai = GetComponent<MonsterAI>();
    }

    public void OnBossAttack()
    {
        hitCounter++;

        if (hitCounter >= data.triggerAfterHits)
        {
            hitCounter = 0;
            CastSkill();
        }
    }

    void CastSkill()
    {
        switch (data.skillType)
        {
            case BossSkillType.FanShot:
                FanShot();
                break;
            case BossSkillType.PiercingCircle:
                PiercingCircle();
                break;
        }
    }

    void FanShot()
    {
        float[] angles = { -20f, 0f, 20f };
        foreach (float a in angles)
        {
            SpawnProjectile(a, false);
        }
    }

    void PiercingCircle()
    {
        SpawnProjectile(0f, true, 1.25f);
    }

    void SpawnProjectile(float angle, bool piercing, float dmgMul = 1f)
    {
        GameObject proj = Instantiate(ai.projectilePrefab, ai.attackPoint.position, Quaternion.identity);
        Projectile p = proj.GetComponent<Projectile>();
        p.damage = Mathf.RoundToInt(ai.monsterHealth.stats.attackDamage * dmgMul);
        p.speed = ai.projectileForce;
        p.SetPiercing(piercing);
        p.SetAngle(angle);
    }
}