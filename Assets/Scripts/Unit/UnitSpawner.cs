using System.Runtime.InteropServices.WindowsRuntime;
using TMPro;
using Unity.Mathematics.Geometry;
using UnityEngine;
using UnityEngine.UI;

public class UnitSpawner : MonoBehaviour
{
    public long costMelee = 100;
    public long costRange = 100;
    public TMP_Text txtCostMelee;
    public TMP_Text txtCostRange;
    public BattleManager battleManager;
    public static UnitSpawner Instance;
    public Sprite[] sp;
    public SpriteRenderer[] img;
    public GameObject[] buyadsMelee;
    public GameObject[] buyadsRange;

    private void Awake()
    {
        Instance = this;
        var shop = EconomyConfig.Instance.unitShop;
        costMelee = shop.startCost.melee;
        costRange = shop.startCost.range;
    }

    public void LoadCost(long cM, long cR) //Làm mới giá tiền mua Unit
    {
        costMelee = cM;
        costRange = cR;
        OnCost();
    }
    public void OnCost() //Hien giá tiền mua Unit
    {
        if (Char.Instance.level > 2)
        {
            bool isEnoughM = Char.Instance.coins >= costMelee;
            bool isEnoughR = Char.Instance.coins >= costRange;
            if (!txtCostMelee.gameObject.activeSelf) txtCostMelee.gameObject.SetActive(true);
            if (!txtCostRange.gameObject.activeSelf) txtCostRange.gameObject.SetActive(true);
            txtCostMelee.SetText(isEnoughM ? Char.FormatMoney(costMelee) : "FREE");
            txtCostRange.SetText(isEnoughR ? Char.FormatMoney(costRange) : "FREE");
            img[0].sprite = sp[isEnoughM ? 0 : 1];
            img[1].sprite = sp[isEnoughR ? 0 : 1];
            img[2].sprite = sp[isEnoughM ? 2 : 3];
            img[3].sprite = sp[isEnoughR ? 2 : 3];
            buyadsMelee[0].SetActive(isEnoughM);
            buyadsMelee[1].SetActive(!isEnoughM);
            buyadsRange[0].SetActive(isEnoughR);
            buyadsRange[1].SetActive(!isEnoughR);
        } else
        {
            if (!txtCostMelee.gameObject.activeSelf) txtCostMelee.gameObject.SetActive(true);
            if (!txtCostRange.gameObject.activeSelf) txtCostRange.gameObject.SetActive(true);
            txtCostMelee.SetText("FREE");
            txtCostRange.SetText("FREE");
            img[0].sprite = sp[0];
            img[1].sprite = sp[0];
            img[2].sprite = sp[2];
            img[3].sprite = sp[2];
        }
    }
    public void UpgradeCost(bool isMelee)
    {
        var shop = EconomyConfig.Instance.unitShop;
        if (Char.Instance.level < shop.increaseAfterLevel) return;
        if (isMelee) costMelee = (long)System.Math.Round(costMelee * shop.costIncreaseRate);
        else costRange = (long)System.Math.Round(costRange * shop.costIncreaseRate);
        OnCost();
    }

    public void SpawnRangeUnit(int level) //Spawn Unit đánh xa
    {
        GridManager grid = GridManager.Instance;
        // Tìm ô trống đầu tiên (từ dưới lên)
        for (int y = 0; y <= 2; y++)
        {
            for (int x = 4; x >= 0; x--)
            {
                if (grid.IsEmpty(x, y))
                {
                    GameObject unitObj = Instantiate(Char.Instance.GetUnitPrefabs(level, false));
                    MonsterHealth unit = unitObj.GetComponent<MonsterHealth>();
                    battleManager.playerTeam.Add(unitObj);
                    Char.Instance.dataMyTeam.Add(unit);
                    unit.SetStats(level);
                    AudioManager.Instance.PlayUnitSound(level, unit.stats.type);
                    grid.Place(unit, x, y);
                    if(Char.Instance.level >= EconomyConfig.Instance.unitShop.increaseAfterLevel) UpgradeCost(false);
                    VFXManager.Instance.Play(VFXType.Spawn, unit.transform.position);
                    return;
                }
            }
        }
    }

    public void SpawnMeleeUnit(int level) //Spawn unit cận chiến
    {
        GridManager grid = GridManager.Instance;
        for (int y = 2; y >= 0; y--)
        {
            for (int x = 0; x <= 4; x++)
            {
                if (grid.IsEmpty(x, y))
                {
                    GameObject unitObj = Instantiate(Char.Instance.GetUnitPrefabs(level, true));
                    MonsterHealth unit = unitObj.GetComponent<MonsterHealth>();
                    battleManager.playerTeam.Add(unitObj);
                    Char.Instance.dataMyTeam.Add(unit);
                    unit.SetStats(level);
                    AudioManager.Instance.PlayUnitSound(level, unit.stats.type);
                    grid.Place(unit, x, y);
                    if (Char.Instance.level >= EconomyConfig.Instance.unitShop.increaseAfterLevel) UpgradeCost(true);
                    VFXManager.Instance.Play(VFXType.Spawn, unit.transform.position);
                    return;
                }
            }
        }
        Debug.Log("Grid full - cannot spawn unit");
    }
}