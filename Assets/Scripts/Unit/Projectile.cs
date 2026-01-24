using UnityEngine;

public class Projectile : MonoBehaviour
{
    public int damage;
    public float lifeTime = 3f;
    public float speed;
    public GameObject enemy;
    private bool isRegistered = false;
    private Vector2 direction;
    private bool useCustomDir = false;
    public void Awake()
    {
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.activeBulletCount++;
            isRegistered = true;
        }
    }
    void Update()
    {
        if (enemy == null || !enemy.activeSelf || !BattleManager.Instance.startPvP || BombPlane.IsOutOfBackground(transform.position))
        {
            Delete();
            return;
        }
        if (BoosterManager.Instance.isOpenPanel) return;
        if (useCustomDir)
        {
            transform.position += (Vector3)(direction * speed * Time.deltaTime);
            transform.rotation = Quaternion.AngleAxis(Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg, Vector3.forward);
        }
        else
        {
            transform.position = Vector2.MoveTowards(transform.position, enemy.transform.position, speed * Time.deltaTime);
            Vector2 dir = enemy.transform.position - transform.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            if (Vector2.Distance(transform.position, enemy.transform.position) < 0.1f)
            {
                HitTarget();
            }
        }
    }
    void HitTarget()
    {
        MonsterHealth hp = enemy.GetComponent<MonsterHealth>();
        if (hp != null)
        {
            hp.TakeDamage(damage);
        }
        if (!isPiercing) Delete();
    }
    public void Delete()
    {
        if (isRegistered && BattleManager.Instance != null)
        {
            BattleManager.Instance.activeBulletCount--;
        }
        Destroy(gameObject);
    }
    public void SetAngle(float angle)
    {
        useCustomDir = true;
        direction = Quaternion.Euler(0, 0, angle) * Vector2.right;
    }
    public bool isPiercing = false;

    public void SetPiercing(bool value)
    {
        isPiercing = value;
    }

}