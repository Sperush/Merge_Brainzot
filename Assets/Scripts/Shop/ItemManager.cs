using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemManager : MonoBehaviour
{
    public bool isMelee; //Kiểm tra xem có phải Unit cận chiến không
    public int cost; //Giá tiền của Unit
    public int level; //level của Unit
    public int dame; //atk của Unit
    public int hp; //hp của Unit
    public string nameItem;
    public TMP_Text txtCost;
    public TMP_Text txtDame;
    public TMP_Text txtHp;
    public TMP_Text txtName;
    public Image spriteRenderer;
    public Sprite[] visualsMelee;
    public Sprite[] visualsRange;
    public GameObject statsUnit;
    private void Awake()
    {
        nameItem = isMelee ? Char.Instance.nameUnitMelee[level - 1] : Char.Instance.nameUnitRange[level - 1];
        Load();
    }
    public void Load()
    {
        SetStats(level);
        if (txtCost != null) txtCost.SetText(cost.ToString());
        if (txtDame != null) txtDame.SetText(dame.ToString());
        if (txtHp != null) txtHp.SetText(hp.ToString());
        if (txtName != null) txtName.SetText((isMelee && !Char.Instance.unlockUnitMelee[level-1]) || (!isMelee && !Char.Instance.unlockUnitRange[level - 1]) ? "Locked":nameItem);
    }
    public void UpdateVisual() //Cập nhật visual cho phù hợp với level Unit
    {
        spriteRenderer.sprite = isMelee? visualsMelee[level - 1]:visualsRange[level-1];
    }
    public void SetStats(int level)
    {
        if (level < 1 || level > 8) return;
        int index = level - 1;
        var cfg = UnitStatsConfig.Instance.units;
        if (isMelee)
        {
            dame = cfg.melee.damage[index];
            hp = cfg.melee.hp[index];
        }
        else
        {
            dame = cfg.range.damage[index];
            hp = cfg.range.hp[index];
        }
        if ((isMelee && !Char.Instance.unlockUnitMelee[index]) || (!isMelee && !Char.Instance.unlockUnitRange[index]))
        {
            spriteRenderer.color = Color.black;
            statsUnit.SetActive(false);
        }
        else
        {
            spriteRenderer.color = Color.white;
            statsUnit.SetActive(true);
        }
        UpdateVisual();
    }

    public void BuyUnit() //Mua unit
    {
        if (!Char.Instance.SubGems(cost)) return;
        PanelManager.Instance.hideSummonPanel();
        if (isMelee)
        {
            UnitSpawner.Instance.SpawnMeleeUnit(level);
        }
        else
        {
            UnitSpawner.Instance.SpawnRangeUnit(level);
        }
    }

}
