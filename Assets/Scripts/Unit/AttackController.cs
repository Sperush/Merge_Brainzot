using UnityEngine;

public class AttackController : MonoBehaviour
{
    public MonsterAI monsterAI;
    public void AttackMelee() //Hàm tấn công của Unit cận chiến
    {
        if (monsterAI.currentTarget != null && !monsterAI.currentTarget.isDead && !monsterAI.monsterHealth.isDead && BattleManager.Instance.isOkPvP())
        {
            AudioManager.Instance.Play(GameSound.meleeAttackSound);
            monsterAI.currentTarget.TakeDamage(monsterAI.monsterHealth.stats.attackDamage);
        }
    }
    public void ForceAttack() //Hàm bắn của Unit đánh xa
    {
        if (monsterAI.currentTarget != null && !monsterAI.currentTarget.isDead && !monsterAI.monsterHealth.isDead && BattleManager.Instance.isOkPvP())
        {
            GameObject proj = Instantiate(monsterAI.projectilePrefab, monsterAI.attackPoint.position, Quaternion.identity);
            monsterAI.projectile = proj;
            Projectile p = proj.GetComponent<Projectile>();
            p.enemy = monsterAI.currentTarget.gameObject;
            p.damage = monsterAI.monsterHealth.stats.attackDamage;
            p.speed = monsterAI.projectileForce;
            AudioManager.Instance.Play(GameSound.rangeAttackSound);
        }
    }
    public void setDie()
    {
        monsterAI.gameObject.SetActive(false);
        monsterAI.animator.SetBool("isDie", false);
    }
}
