using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.XR;

public class MonsterHealth : MonoBehaviour
{
    public int gridX; //Tọa độ x của Unit trong grid
    public int gridY; //Tọa độ y của Unit trong grid
    [Header("Visual")]
    public SpriteRenderer spriteRenderer;
    public Sprite[] visuals;
    public MonsterStats stats;
    public MonsterAI monsterAI;
    public HPBar hpBar;
    public GameObject damageTextPrefab;
    public Transform textSpawnPoint;
    public int damageInSecond=0; //Giá dame nhận được trong 1 giây
    public float timeShowDameTxt = 1f; //Delay hiển thị dame

    public float yOffset = 0.2f;
    void Awake()
    {
        stats.currentHP = stats.maxHP;
        hpBar.SetHP(1f);
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();
        UpdateVisual();
    }
    private void Update()
    {
        if(timeShowDameTxt > 0) timeShowDameTxt -= Time.deltaTime;
    }
    public void TakeDamage(int dmg) //Hàm tính lượng hp còn lại sau khi bị attack với sát thương dmg
    {
        if (stats.currentHP > 0) dmg = Mathf.Min(dmg, stats.currentHP);
        damageInSecond += dmg;
        stats.currentHP -= dmg;
        if (hpBar != null) hpBar.SetHP((stats.currentHP * 1.0f) / (stats.maxHP * 1.0f));
        if (stats.currentHP <= 0) Die();
        if (CompareTag("Enemy") && timeShowDameTxt <= 0 && BattleManager.Instance.startPvP)
        {
            ShowDamage(damageInSecond);
        }
    }
    void ShowDamage(int damage) //Hàm hiển thị dame
    {
        GameObject textObj = Instantiate(damageTextPrefab, textSpawnPoint.position, Quaternion.identity, textSpawnPoint);
        DamageText dmgText = textObj.GetComponent<DamageText>();
        dmgText.SetText(Char.FormatMoney(damage));
        dmgText.DamageSize(damage);
        timeShowDameTxt = 1f;
        damageInSecond = 0;
    }
    void Die()
    {
        gameObject.SetActive(false);
    }

    public void LevelUp(int count) //Nâng cấp level của Unit
    {
        stats.level += count;
        SetStats(stats.level);
        UpdateVisual();
        Bounds b = spriteRenderer.bounds;
        Vector2 pos = transform.position;
        pos.y = b.max.y + yOffset;
        hpBar.transform.position = pos;
    }
    public void SetStats(int level)
    {
        var cfg = UnitStatsConfig.Instance.units;
        int index = Mathf.Clamp(level - 1, 0, cfg.melee.hp.Length - 1);
        if (stats.type == MonsterType.Melee)
        {
            stats.attackDamage = cfg.melee.damage[index];
            stats.maxHP = cfg.melee.hp[index];
            stats.attackSpeed = cfg.melee.attackSpeed[index];
            stats.attackRange = cfg.melee.attackRange[index];
            stats.moveSpeed = cfg.melee.moveSpeed[index];
        }
        else
        {
            if (!BattleManager.Instance.arrRange.Contains(monsterAI))
            {
                BattleManager.Instance.arrRange.Add(monsterAI);
            }
            stats.attackDamage = cfg.range.damage[index];
            stats.maxHP = cfg.range.hp[index];
            stats.attackSpeed = cfg.range.attackSpeed[index];
            stats.attackRange = cfg.range.attackRange[index];
            stats.moveSpeed = cfg.range.moveSpeed[index];
        }
        // FAIL SAFE BUFF
        //if (LoseTracker.IsFailSafeActive())
        //{
        //    var fs = FailSafeConfig.Instance.failSafe;
        //    stats.attackDamage *= fs.damageMultiplier;
        //    stats.maxHP *= fs.hpMultiplier;
        //}
        stats.currentHP = stats.maxHP;
        hpBar.SetHP(1f);
    }

    public void UpdateVisual() //Cập nhật visual cho phù hợp với level Unit
    {
        spriteRenderer.sprite = visuals[stats.level - 1];
    }
    public void SetGridPos(int x, int y) //Lưu vị trí của Unit
    {
        gridX = x;
        gridY = y;
    }
    public void ResetStatus() //Trả về trạng thái chuẩn bị
    {
        SetStats(stats.level);
        MonsterAI ai = GetComponent<MonsterAI>();
        ai.currentTarget = null;
        ai.attackTimer = 0f;
        GridManager.Instance.Place(this, gridX, gridY);
        spriteRenderer.flipX = false;
        damageInSecond = 0;
        timeShowDameTxt = 1f;
    }
}