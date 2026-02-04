using DG.Tweening;
using GIE;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using Unity.Properties;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.UI;
[Serializable]
public class DataUnit //Dữ liệu của mỗi Unit
{
    public string type;
    public int level;
    public int maxHP;
    public int attackDamage;
    public float attackSpeed;   // thời gian giữa các đòn
    public float attackRange;   // range đánh / bắn
    public float moveSpeed;
    public int gridX;
    public int gridY;
}
[Serializable]
public class SaveData //Dữ liệu cần lưu
{
    public long coins;
    public int gems;
    public int level;
    public int boosterFreeze;
    public int boosterBomp;
    public int coutStreak;
    public int giftCycle;
    public long costMelee;
    public long costRange;
    public int freeSpinLeft;
    public List<bool> giftCollected;
    public Team dataMyTeam;
    public List<bool> unlockUnitMelee;
    public List<bool> unlockUnitRange;
    public string LastClaimTime;
    public int currentDayIndex;
}
[Serializable]
public class Team
{
    public List<DataUnit> units; //Danh sách Unit hiện có
}
[System.Serializable]
public class DataSave
{
    public int level;
    public Team enemyTeam;
}
public class Char : MonoBehaviour
{
    public int level; //Level màn chơi của người chơi 
    public long coins; //Số tiền của người chơi 
    public int gems; //Số tiền của người chơi 
    public int boosterFreeze;
    public int boosterBomp;
    public int freeSpinLeft;
    public TMP_Text txtCoins; 
    public TMP_Text txtGems;
    public TMP_Text txtLevel;
    public List<MonsterHealth> dataMyTeam = new List<MonsterHealth>();
    public List<bool> unlockUnitMelee = new List<bool>() { true, false, false, false, false, false, false, false };
    public List<bool> unlockUnitRange = new List<bool>() { true, false, false, false, false, false, false, false };
    public List<bool> giftCollected = new List<bool>() { false, false, false };
    public static Char Instance;
    public GameObject[] meleePrefabs;
    public GameObject[] rangePrefabs;
    public string[] nameUnitMelee;
    public string[] nameUnitRange;
    public List<ItemManager> itemMelee;
    public List<ItemManager> itemRange;
    public List<ItemManager> myitemMelee;
    public List<ItemManager> myitemRange;
    public int coutStreak;
    public int giftCycle;
    public string LastClaimTime = DateTime.MinValue.ToString();
    public int currentDayIndex;
    public bool canClaimToday;
    public Image imgMusic;
    public Image imgSound;

    public int activePointerId = -999;
    private void Awake()
    {
        GameLog.Log("session_start");
        Instance = this;
        imgMusic.sprite = BackgroundMusic.Instance.MusicSprite[BackgroundMusic.Instance.audioMusic.mute ? 1:0];
        imgSound.sprite = AudioManager.Instance.sp[AudioManager.Instance.isMuted ? 1 : 0];
    }
    public GameObject GetUnitPrefabs(int level, bool isMelee)
    {
        var arr = isMelee ? meleePrefabs : rangePrefabs;
        level = Mathf.Clamp(level - 1, 0, arr.Length - 1);
        return arr[level];
    }

    public void ToggleMute(bool isMusic)
    {
        if (isMusic) BackgroundMusic.Instance.MuteMusic();
        else AudioManager.Instance.ToggleMute();
    }
    void Start()
    {
        Load(Application.persistentDataPath + "/save.json");
        txtCoins.SetText(FormatMoney(coins));
        txtGems.SetText(gems.ToString());
        if (level <= 1) TutorialController.Instance.StartPhase1();
        else if(level == 2) TutorialController.Instance.StartPhase2_Merge();
        CheckStatus();
        BattleManager.Instance.LoadLevel(true);
        AddStreakBar(0);
        UpdateBooster();
    }
    public void AddStreakBar(int a)
    {
        coutStreak += a;
        StreakManager.Instance.LoadBar();
    }

    private void CheckStatus()
    {
        DateTime now = DateTime.UtcNow;
        DateTime today = now.Date;
        DateTime lastClaimDate = DateTime.MinValue;
        bool hasClaimBefore = false;
        if (!string.IsNullOrEmpty(LastClaimTime) && long.TryParse(LastClaimTime, out long ticks))
        {
            lastClaimDate = new DateTime(ticks, DateTimeKind.Utc);
            hasClaimBefore = true;
        }
        // Chống chỉnh giờ máy
        if (hasClaimBefore && lastClaimDate > now)
        {
            Debug.LogWarning("Time cheat detected");
            canClaimToday = false;
            return;
        }
        // Lần đầu chơi
        if (!hasClaimBefore)
        {
            currentDayIndex = 0;
            canClaimToday = true;
            return;
        }
        double daysDiff = (today - lastClaimDate.Date).TotalDays;
        if (daysDiff < 1)
        {
            // Đã nhận hôm nay
            canClaimToday = false;
        }
        else if (daysDiff < 2)
        {
            // Ngày tiếp theo
            currentDayIndex++;
            canClaimToday = true;
        }
        else
        {
            currentDayIndex = 0;
            canClaimToday = true;
            Debug.Log("Đã reset về ngày 1 do đứt chuỗi");
        }
        if (currentDayIndex > 6) currentDayIndex = 0;
    }

    public void Save(string path) //Lưu lại dữ liệu của người chơi
    {
        SaveData saveData = new SaveData();
        saveData.level = level;
        saveData.coins = coins;
        saveData.gems = gems;
        saveData.coutStreak = coutStreak;
        saveData.boosterBomp = boosterBomp;
        saveData.boosterFreeze = boosterFreeze;
        saveData.freeSpinLeft = freeSpinLeft;
        saveData.costMelee = UnitSpawner.Instance.costMelee;
        saveData.costRange = UnitSpawner.Instance.costRange;
        saveData.giftCollected = giftCollected;
        saveData.currentDayIndex = currentDayIndex;
        saveData.LastClaimTime = LastClaimTime;
        saveData.giftCycle = giftCycle;
        List<DataUnit> data = new List<DataUnit>();
        foreach(var m in dataMyTeam)
        {
            if (m == null) continue;
            data.Add(formatTodata(m));
        }
        saveData.dataMyTeam = new Team
        {
            units = data
        };
        saveData.unlockUnitMelee = unlockUnitMelee;
        saveData.unlockUnitRange = unlockUnitRange;
        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(path, json);
        Debug.Log("Saved: " + path);
    }
    public DataUnit formatTodata(MonsterHealth m) //Hàm chuyển đổi dữ liệu MonsterHealth sang DataUnit
    {
        return new DataUnit
        {
            type = m.stats.type.ToString(),
            level = m.stats.level,
            maxHP = m.stats.maxHP,
            attackDamage = m.stats.attackDamage,
            attackSpeed = m.stats.attackSpeed,
            attackRange = m.stats.attackRange,
            moveSpeed = m.stats.moveSpeed,
            gridX = m.gridX,
            gridY = m.gridY
        };
    }
    public static string FormatMoney(long value)
    {
        if (value >= 1_000_000_000)
            return (value / 1_000_000_000).ToString() + "B";
        if (value >= 1_000_000)
            return (value / 1_000_000).ToString() + "M";
        if (value >= 1_000)
            return (value / 1_000).ToString() + "K";

        return value.ToString();
    }

    public void Load(string path) //Load dữ liệu của người chơi
    {
        if (!File.Exists(path))
        {
            Debug.Log("No save file");
            //LoadMyTeamNewBie();
            return;
        }
        string json = File.ReadAllText(path);
        SaveData saveData = JsonUtility.FromJson<SaveData>(json);
        level = saveData.level;
        coins = saveData.coins;
        gems = saveData.gems;
        boosterFreeze = saveData.boosterFreeze;
        boosterBomp = saveData.boosterBomp;
        coutStreak = saveData.coutStreak;
        freeSpinLeft = saveData.freeSpinLeft;
        giftCollected = saveData.giftCollected;
        unlockUnitMelee = saveData.unlockUnitMelee;
        unlockUnitRange = saveData.unlockUnitRange;
        LastClaimTime = saveData.LastClaimTime;
        currentDayIndex = saveData.currentDayIndex;
        giftCycle = saveData.giftCycle;
        if (level >= EconomyConfig.Instance.unitShop.increaseAfterLevel) UnitSpawner.Instance.LoadCost(saveData.costMelee, saveData.costRange);
        foreach (var m in saveData.dataMyTeam.units)
        {
            GameObject obj = Instantiate(GetUnitPrefabs(m.level, m.type == MonsterType.Melee.ToString()));
            MonsterHealth mh = obj.GetComponent<MonsterHealth>();
            mh.SetGridPos(m.gridX, m.gridY);
            mh.SetStats(m.level);
            dataMyTeam.Add(mh);
            BattleManager.Instance.playerTeam.Add(obj);
            GridManager.Instance.Place(mh, mh.gridX, mh.gridY);
        }
        Debug.Log("Loaded game");
    }
    public void OnApplicationPause(bool pause) //Lưu dữ liệu khi rời khỏi game(chưa out game)
    {
        GameLog.Log("session_end", new
        {
            level = level,
            coins = coins
        });
        if (pause && level > 2) Save(Application.persistentDataPath + "/save.json");
    }
    private void OnApplicationQuit() //Lưu dữ liệu khi out game
    {
        GameLog.Log("session_end", new
        {
            level = level,
            coins = coins
        });
        if (level > 2) Save(Application.persistentDataPath + "/save.json");
    }
    public int mItemNumber = 10;
    //public void OnClickMoney(RectTransform from_where=null)
    //{
    //    if (BattleManager.Instance.winPanel.activeSelf && (level-1) % 5 == 0) GetItemEffect.mInstance.GetItem(TypeItem.Gems, mItemNumber, from_where, null);
    //}
    //public void OnClickGems(RectTransform from_where = null)
    //{
    //    GetItemEffect.mInstance.GetItem(TypeItem.Gems, mItemNumber, from_where, null);
    //}
    public bool SubCoins(long a, bool isMelee=true) //Trừ coin của người chơi
    {
        if (a > coins)
        {
            //RewardedAds.Instance.LoadRewardedAd((isSuccess) =>
            //{
            //    if (isSuccess)
            //    {
            if (isMelee)
            {
                UnitSpawner.Instance.SpawnMeleeUnit(1);
                UnitSpawner.Instance.SpawnMeleeUnit(1);
            }
            else
            {
                UnitSpawner.Instance.SpawnRangeUnit(1);
                UnitSpawner.Instance.SpawnRangeUnit(1);
            }
            //    }
            //    else
            //    {
            //        Debug.Log("Người chơi tắt ngang hoặc lỗi Ad, không thưởng.");
            //    }
            //});
            return false;
        }
        coins -= a;
        coins = Max(coins, 0);
        txtCoins.SetText(FormatMoney(coins));
        return true;
    }

    private Tween tween;
    public void AddCoins(long a) //Thêm coin của người chơi
    {
        if (a <= 0) return;
        long startCoins = coins;
        long endCoins = coins + a;
        float flyTime = GetItemEffect.mInstance.GetItem(TypeItem.Coins, mItemNumber);
        Play(startCoins, endCoins, flyTime);
        coins = endCoins;
        coins = Min(coins, long.MaxValue);
        txtCoins.SetText(FormatMoney(coins));
    }
    public void Play(long start, long end, float duration, bool isGem=false)
    {
        TMP_Text txt = isGem ? txtGems : txtCoins;
        tween?.Kill();
        tween = DOTween.To(() => start,x =>
        {
            start = x;
            txt.SetText(FormatMoney(start));
        }, end, duration).SetEase(Ease.Linear);
    }
    public bool SubBooster(TypeBooster type) //Trừ booster của người chơi
    {
        if (type == TypeBooster.Freeze)
        {
            if (boosterFreeze <= 0)
            {
                BoosterManager.Instance.isOpenPanel = true;
                foreach (var m in BoosterManager.Instance.itemBoosters)
                {
                    m.Init(true);
                }
                PanelManager.Instance.OpenPanel(PanelManager.Instance.BuyBoosterPanel);
                return false;
            }
            boosterFreeze--;
            boosterFreeze = Mathf.Max(boosterFreeze, 0);
        }
        else
        {
            if (boosterBomp <= 0)
            {
                BoosterManager.Instance.isOpenPanel = true;
                foreach (var m in BoosterManager.Instance.itemBoosters)
                {
                    m.Init(false);
                }
                PanelManager.Instance.OpenPanel(PanelManager.Instance.BuyBoosterPanel);
                return false;
            }
            boosterBomp--;
            boosterBomp = Mathf.Max(boosterBomp, 0);
        }
        UpdateBooster();
        return true;
    }
    public void AddBooster(TypeBooster type, int a) //Thêm coin của người chơi
    {
        if(type == TypeBooster.Freeze)
        {
            boosterFreeze += a;
            boosterFreeze = Mathf.Min(boosterFreeze, int.MaxValue);
        } else
        {
            boosterBomp += a;
            boosterBomp = Mathf.Min(boosterBomp, int.MaxValue);
        }
        UpdateBooster();
    }
    public void UpdateBooster()
    {
        if (boosterFreeze > 0)
        {
            BoosterManager.Instance.img[0].color = Color.cyan;
            BoosterManager.Instance.txtCount[0].SetText(FormatMoney(boosterFreeze));
            BoosterManager.Instance.txtCount[0].gameObject.SetActive(true);
            BoosterManager.Instance.booster[0].SetActive(false);
        } else
        {
            BoosterManager.Instance.img[0].color = Color.green;
            BoosterManager.Instance.booster[0].SetActive(true);
            BoosterManager.Instance.txtCount[0].gameObject.SetActive(false);
        }
        if (boosterBomp > 0)
        {
            BoosterManager.Instance.img[1].color = Color.cyan;
            BoosterManager.Instance.txtCount[1].SetText(FormatMoney(boosterBomp));
            BoosterManager.Instance.txtCount[1].gameObject.SetActive(true);
            BoosterManager.Instance.booster[1].SetActive(false);
        }
        else
        {
            BoosterManager.Instance.img[1].color = Color.green;
            BoosterManager.Instance.booster[1].SetActive(true);
            BoosterManager.Instance.txtCount[1].gameObject.SetActive(false);
        }
    }
    public long Min(long a, long b)
    {
        return (a < b) ? a : b;
    }
    public long Max(long a, long b)
    {
        return (a > b) ? a : b;
    }
    public bool SubGems(int a) //Trừ gem của người chơi
    {
        if (a > gems)
        {
            Noti.Instance.Show("not_enough_gem");
            return false;
        }
        gems -= a;
        gems = Mathf.Max(gems, 0);
        txtGems.SetText(gems.ToString());
        return true;
    }
    public void AddGems(int a) //Thêm gem của người chơi
    {
        if (a <= 0) return;
        long startGems = gems;
        int endGems = gems + a;
        float flyTime = GetItemEffect.mInstance.GetItem(TypeItem.Gems, mItemNumber);
        Play(startGems, endGems, flyTime, true);
        gems = endGems;
        gems = Mathf.Min(gems, int.MaxValue);
        txtGems.SetText(gems.ToString());
    }
}