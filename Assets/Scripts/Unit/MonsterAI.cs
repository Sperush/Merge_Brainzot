using System.Collections;
using UnityEngine;
using UnityEngine.UI;

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

    public MonsterHealth currentTarget;
    public GameObject projectile;
    public Animator animator;
    public Transform visual;
    public Transform visualRoot;
    public Sprite imgProjectile;
    public GameObject objFreeze;
    private bool isUpCached;
    void Update()
    {
        if (!canAttack()) return;
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
        if (currentTarget == null || currentTarget.isDead)
        {
            FindNearestTarget();
            return;
        }
        if (currentTarget == null || currentTarget.isDead || monsterHealth.isDead)
            return;
        if (monsterHealth.stats.type == MonsterType.Melee)
        {
            HandleMelee();
        }
        else
        {
            HandleRanged();
        }
    }
    public bool canAttack()
    {
        return !isFrozen && !BoosterManager.Instance.isOpenPanel && !monsterHealth.isDead;
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
        objFreeze.SetActive(true);
        // Optional: animation / effect
        yield return new WaitForSeconds(duration);
        objFreeze.SetActive(false);
        isFrozen = false;
    }
    public void ResetAIState()
    {
        currentTarget = null;
        isReady = false;
        isUpCached = false;
    }

    public bool isSuccessDistance()
    {
        Vector2 myPos = transform.position;
        Vector2 targetPos = currentTarget.transform.position;
        return Mathf.Abs(myPos.x - targetPos.x) <= monsterHealth.stats.attackRange+xTolerance && Mathf.Abs(myPos.y - targetPos.y) <= laneTolerance;
    }

    void HandleMelee() //AI của Unit cận chiến gồm: di chuyển và tấn công và xoay hướng
    {
        if (currentTarget == null || currentTarget.isDead || monsterHealth.isDead || animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack")) return;

        Vector2 myPos = transform.position;
        Vector2 targetPos = currentTarget.transform.position;

        float attackRange = monsterHealth.stats.attackRange;
        float moveSpeed = monsterHealth.stats.moveSpeed;

        float sideDir = targetPos.x > myPos.x ? -1f : 1f;
        Vector3 scal = visual.localScale;
        bool isFlip = targetPos.x < myPos.x;
        if (isFlip && scal.x > 0 || !isFlip && scal.x < 0)
        {
            scal.x = -scal.x;
        }
        visual.localScale = scal;
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
            animator.SetBool("isRunning", true);
            MoveTo(desiredPos, moveSpeed);
            return;
        }
        // ===== CASE 2: QUÁ XA =====
        if (distanceX > attackRange + xTolerance)
        {
            animator.SetBool("isRunning", true);
            MoveTo(desiredPos, moveSpeed);
            return;
        }
        // ===== CASE 3: QUÁ GẦN -> LÙI SANG BÊN =====
        if (distanceX < attackRange - xTolerance)
        {
            animator.SetBool("isRunning", true);
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
        if (animator.GetCurrentAnimatorStateInfo(0).IsTag("Run")) animator.SetBool("isRunning", false);
        // ===== CASE 4: ĐÚNG KHOẢNG CÁCH -> ĐÁNH =====
        if (!isReady && !BattleManager.Instance.arrUnitReady.Exists(m => m == gameObject))
        {
            BattleManager.Instance.arrUnitReady.Add(gameObject);
            isReady = true;
        }
        if (currentTarget != null && currentTarget.gameObject.activeSelf && BattleManager.Instance.isOkPvP() && !monsterHealth.isDead && isSuccessDistance())
        {
            PlayAttackAnimation();
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
        if (HasTarget() && !monsterHealth.isDead)
        {
            bool isUp = monsterHealth.gridY < currentTarget.gridY;
            if (isUp != isUpCached)
            {
                isUpCached = isUp;
                animator.SetBool("isUp", isUp);
            }
            Vector3 scal = visual.localScale;
            bool isFlip = currentTarget.transform.position.x < transform.position.x;
            if (isFlip && scal.x > 0 || !isFlip && scal.x < 0)
            {
                scal.x = -scal.x;
            }
            visual.localScale = scal;
        }
    }

    void FindNearestTarget() //Tìm kiếm địch gần nhất
    {
        Collider2D[] cols = Physics2D.OverlapCircleAll(transform.position, AIConfig.Instance.targeting.searchRadius, enemyLayer);
        float minDist = float.MaxValue;
        MonsterHealth nearest = null;
        MonsterHealth mh;
        Vector3 myPos = transform.position;
        foreach (var col in cols)
        {
            mh = col.GetComponent<MonsterHealth>();
            if (mh.isDead) continue;
            Vector3 diff = col.transform.position - myPos;
            float sqrDist = diff.sqrMagnitude;
            if (sqrDist < minDist)
            {
                minDist = sqrDist;
                nearest = mh;
            }
        }
        currentTarget = nearest;
    }
    public bool HasTarget()
    {
        if (currentTarget == null || currentTarget.isDead)
        {
            FindNearestTarget();
        }
        return currentTarget != null;
    }
    public void PlayAttackAnimation()
    {
        if (!monsterHealth.isDead && currentTarget != null && !currentTarget.isDead && !animator.GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
        {
            animator.SetTrigger("attack");
        }
    }

}