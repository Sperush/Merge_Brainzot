using System.Collections;
using UnityEngine;

public class MonsterAI : MonoBehaviour
{
    [Header("Booster Status")]
    public bool isFrozen;

    public bool isReady;
    public float laneTolerance = 0.2f;   // sai lệch Y cho phép (cùng lane)
    public float xTolerance = 0.5f;      // sai lệch X cho phép (đứng ngang)
    public MonsterHealth monsterHealth;
    public LayerMask enemyLayer;

    public Transform attackPoint;
    public GameObject projectilePrefab;
    public float projectileForce = 10f; //Tốc độ bay của đạn

    public float attackTimer;  //Delay đánh
    public Transform currentTarget;
    public GameObject projectile;

    public Transform visualRoot;
    public SpriteRenderer sprite;

    void Update()
    {
        if (isFrozen) return;
        var cfg = AIConfig.Instance;
        if (monsterHealth.stats.type == MonsterType.Melee)
        {
            if(laneTolerance != cfg.melee.laneTolerance) laneTolerance = cfg.melee.laneTolerance;
            if (xTolerance != cfg.melee.xTolerance) xTolerance = cfg.melee.xTolerance;
        }
        else
        {
            if (projectileForce != cfg.range.projectileForce) projectileForce = cfg.range.projectileForce;
        }
        if (currentTarget == null || !currentTarget.gameObject.activeSelf)
        {
            FindNearestTarget();
            return;
        }
        if (currentTarget == null || !currentTarget.gameObject.activeSelf)
            return;
        if (attackTimer > 0) attackTimer -= Time.deltaTime;
        if (monsterHealth.stats.type == MonsterType.Melee)
        {
            HandleMelee();
        }
        else
        {
            HandleRanged();
        }
    }
    public void Freeze(float duration)
    {
        if (!gameObject.activeSelf) return;
        StopAllCoroutines();
        StartCoroutine(FreezeCoroutine(duration));
    }

    IEnumerator FreezeCoroutine(float duration)
    {
        isFrozen = true;

        // Optional: animation / effect
        yield return new WaitForSeconds(duration);

        isFrozen = false;
    }

    void HandleMelee() //AI của Unit cận chiến gồm: di chuyển và tấn công và xoay hướng
    {
        if (currentTarget == null || !currentTarget.gameObject.activeSelf) return;

        Vector2 myPos = transform.position;
        Vector2 targetPos = currentTarget.position;

        float attackRange = monsterHealth.stats.attackRange;
        float moveSpeed = monsterHealth.stats.moveSpeed;

        float sideDir = targetPos.x > myPos.x ? -1f : 1f;
        sprite.flipX = targetPos.x < myPos.x;
        Vector2 desiredPos = new Vector2(
            targetPos.x + sideDir * attackRange,
            targetPos.y
        );

        desiredPos = GridManager.Instance.ClampToGrid(desiredPos);

        float distanceX = Mathf.Abs(myPos.x - targetPos.x);
        bool sameLane = Mathf.Abs(myPos.y - targetPos.y) <= laneTolerance;

        // ===== CASE 1: KHÔNG CÙNG LANE =====
        if (!sameLane)
        {
            MoveTo(desiredPos, moveSpeed);
            return;
        }
        // ===== CASE 2: QUÁ XA =====
        if (distanceX > attackRange + xTolerance)
        {
            MoveTo(desiredPos, moveSpeed);
            return;
        }
        // ===== CASE 3: QUÁ GẦN -> LÙI SANG BÊN =====
        if (distanceX < attackRange - xTolerance)
        {
            float dirX;

            // 🔒 TRƯỜNG HỢP TRÙNG X (hoặc rất gần)
            if (Mathf.Abs(myPos.x - targetPos.x) < 0.001f)
            {
                // Quy ước cứng: Player & Enemy lùi ngược nhau
                dirX = CompareTag("Enemy") ? 1f : -1f;
            }
            else
            {
                // Bình thường: lùi ra xa target
                dirX = myPos.x < targetPos.x ? -1f : 1f;
            }

            float epsilon = Mathf.Max(0.05f, Mathf.Abs(attackRange - xTolerance - distanceX));

            // 🔍 Nếu đang ở biên & lùi ra ngoài → đổi hướng
            if (GridManager.Instance.IsNearEdgeX(myPos.x, epsilon))
            {
                // chỉ đổi nếu đang lùi RA biên
                float minX = GridManager.Instance.MinX;
                float maxX = GridManager.Instance.MaxX;

                if ((myPos.x <= minX + epsilon && dirX < 0) ||
                    (myPos.x >= maxX - epsilon && dirX > 0))
                {
                    dirX *= -1f;
                }
            }

            Vector2 backPos = new Vector2(
                myPos.x + dirX * moveSpeed * Time.deltaTime,
                myPos.y
            );

            transform.position = GridManager.Instance.ClampToGrid(backPos);
            return;
        }


        // ===== CASE 4: ĐÚNG KHOẢNG CÁCH -> ĐÁNH =====
        if (!isReady && !BattleManager.Instance.arrUnitReady.Exists(m => m == gameObject))
        {
            BattleManager.Instance.arrUnitReady.Add(gameObject);
            isReady = true;
        }
        if (attackTimer <= 0f && currentTarget != null && currentTarget.gameObject.activeSelf && BattleManager.Instance.isOkPvP())
        {
            AttackMelee();
            attackTimer = monsterHealth.stats.attackSpeed;
        }
    }

    void MoveTo(Vector2 targetPos, float speed) //Hàm di chuyển tới vị trí targetPos với tốc độ speed
    {
        transform.position = Vector2.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);
    }

    void HandleRanged()  //Ai của Unit đánh xa gồm: tấn công và xoay hướng
    {
        if (!isReady && !BattleManager.Instance.arrUnitReady.Exists(m => m == gameObject))
        {
            BattleManager.Instance.arrUnitReady.Add(gameObject);
            isReady = true;
        }
        if (HasTarget())
        {
            sprite.flipX = currentTarget.position.x < transform.position.x;
        }
        //if (attackTimer <= 0 && projectile == null && currentTarget != null && currentTarget.gameObject.activeSelf && BattleManager.Instance.isOkPvP())
        //{
        //    sprite.flipX = currentTarget.position.x < transform.position.x;
        //    Shoot();
        //    attackTimer = monsterHealth.stats.attackSpeed;
        //}
    }

    void FindNearestTarget() //Tìm kiếm địch gần nhất
    {
        Collider2D[] cols = Physics2D.OverlapCircleAll(transform.position, AIConfig.Instance.targeting.searchRadius, enemyLayer);
        float minDist = Mathf.Infinity;
        Transform nearest = null;
        foreach (var col in cols)
        {
            float d = Vector2.Distance(transform.position, col.transform.position);
            if (d < minDist)
            {
                minDist = d;
                nearest = col.transform;
            }
        }
        currentTarget = nearest;
    }
    
    void AttackMelee() //Hàm tấn công của Unit cận chiến
    {
        MonsterHealth hp = currentTarget.GetComponent<MonsterHealth>();
        AudioManager.Instance.Play(GameSound.meleeAttackSound);
        if (hp != null)
        {
            hp.TakeDamage(monsterHealth.stats.attackDamage);
        }
    }
    public bool HasTarget()
    {
        if (currentTarget == null || !currentTarget.gameObject.activeSelf)
        {
            FindNearestTarget();
        }
        return currentTarget != null;
    }
    public void ForceAttack() //Hàm bắn của Unit đánh xa
    {
        GameObject proj = Instantiate(projectilePrefab, attackPoint.position, Quaternion.identity);
        projectile = proj;
        Projectile p = proj.GetComponent<Projectile>();
        p.enemy = currentTarget.gameObject;
        p.damage = monsterHealth.stats.attackDamage;
        p.speed = projectileForce;
        AudioManager.Instance.Play(GameSound.rangeAttackSound);
    }
}