using UnityEngine;

public class Projectile : MonoBehaviour
{
    public int damage;
    public float lifeTime = 3f;
    public float speed;
    public GameObject enemy;
    private bool isRegistered = false;
    public void Awake()
    {
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.activeBulletCount++;
            isRegistered = true;
        }
        Destroy(gameObject, lifeTime);
    }
    void Update()
    {
        if (enemy == null || !enemy.activeSelf || !BattleManager.Instance.startPvP)
        {
            Delete();
            return;
        }
        transform.position = Vector2.MoveTowards(transform.position, enemy.transform.position, speed * Time.deltaTime);
        Vector2 dir = enemy.transform.position - transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        if (Vector2.Distance(transform.position, enemy.transform.position) < 0.1f)
        {
            HitTarget();
        }
    }
    void HitTarget()
    {
        MonsterHealth hp = enemy.GetComponent<MonsterHealth>();
        if (hp != null)
        {
            hp.TakeDamage(damage);
        }
        Delete();
    }
    public void Delete()
    {
        if (isRegistered && BattleManager.Instance != null)
        {
            BattleManager.Instance.activeBulletCount--;
        }
        Destroy(gameObject);
    }
}