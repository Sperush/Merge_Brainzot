using System.Collections;
using UnityEngine;
using UnityEngine.UI;

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
            p.img.sprite = monsterAI.imgProjectile;
            AudioManager.Instance.Play(GameSound.rangeAttackSound);
        }
    }
    public void setDie()
    {
        StartCoroutine(DieRoutine());
    }
    IEnumerator DieRoutine()
    {
        // 1️⃣ Tắt AI trước
        monsterAI.enabled = false;
        // 2️⃣ Đợi animator + SpriteSkin chạy xong frame hiện tại
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        monsterAI.animator.enabled = false;
        var skin = GetComponentsInChildren<UnityEngine.U2D.Animation.SpriteSkin>();
        foreach(var m in skin)
        {
            if (m != null) m.enabled = false;
        }
        // 3️⃣ Reset animator
        //monsterAI.animator.Rebind();
        //monsterAI.animator.Update(0f);

        // 4️⃣ Disable object
        monsterAI.gameObject.SetActive(false);
    }

}
