using System.Runtime.InteropServices.WindowsRuntime;
using TMPro;
using Unity.Mathematics.Geometry;
using UnityEngine;

public class UnitSpawner : MonoBehaviour
{
    public long costMelee = 100;
    public long costRange = 100;
    public TMP_Text txtCostMelee;
    public TMP_Text txtCostRange;
    public BattleManager battleManager;
    public GameObject rangeUnitPrefab;
    public GameObject meleeUnitPrefab;
    public static UnitSpawner Instance;


    private void Awake()
    {
        Instance = this;
        var shop = EconomyConfig.Instance.unitShop;
        costMelee = shop.startCost.melee;
        costRange = shop.startCost.range;
        OnCost();
    }

    public void LoadCost(long cM, long cR) //Làm mới giá tiền mua Unit
    {
        costMelee = cM;
        costRange = cR;
        OnCost();
    }
    public void OnCost() //Hien giá tiền mua Unit
    {
        if (!txtCostMelee.gameObject.activeSelf) txtCostMelee.gameObject.SetActive(true);
        if (!txtCostRange.gameObject.activeSelf) txtCostRange.gameObject.SetActive(true);
        txtCostMelee.SetText(Char.FormatMoney(costMelee));
        txtCostRange.SetText(Char.FormatMoney(costRange));
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
                    GameObject unitObj = Instantiate(rangeUnitPrefab);
                    battleManager.playerTeam.Add(unitObj);
                    MonsterHealth unit = unitObj.GetComponent<MonsterHealth>();
                    Char.Instance.dataMyTeam.Add(unit);
                    unit.LevelUp(level);
                    AudioManager.Instance.PlayUnitSound(level, unit.stats.type);
                    grid.Place(unit, x, y);
                    if(Char.Instance.level >= EconomyConfig.Instance.unitShop.increaseAfterLevel) UpgradeCost(false);
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
                    GameObject unitObj = Instantiate(meleeUnitPrefab);
                    battleManager.playerTeam.Add(unitObj);
                    MonsterHealth unit = unitObj.GetComponent<MonsterHealth>();
                    Char.Instance.dataMyTeam.Add(unit);
                    unit.LevelUp(level);
                    AudioManager.Instance.PlayUnitSound(level, unit.stats.type);
                    grid.Place(unit, x, y);
                    if (Char.Instance.level >= EconomyConfig.Instance.unitShop.increaseAfterLevel) UpgradeCost(true);
                    return;
                }
            }
        }
        Debug.Log("Grid full - cannot spawn unit");
    }
}