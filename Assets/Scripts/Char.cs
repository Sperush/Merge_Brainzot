using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
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
    public GameObject meleePrefabs;
    public GameObject rangePrefabs;
    public string[] nameUnitMelee;
    public string[] nameUnitRange;
    public List<ItemManager> itemMelee;
    public List<ItemManager> itemRange;
    public int coutStreak;
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
        DateTime lastClaimDate = DateTime.MinValue;
        if (!string.IsNullOrEmpty(LastClaimTime)) lastClaimDate = DateTime.Parse(LastClaimTime);
        DateTime today = DateTime.Now.Date;
        double daysDiff = (today - lastClaimDate).TotalDays;
        if (daysDiff < 1)
        {
            // Đã nhận hôm nay rồi
            canClaimToday = false;
        }
        else if (daysDiff >= 1 && daysDiff < 2)
        {
            currentDayIndex++;
            // Đúng là ngày tiếp theo (Hôm qua nhận, hôm nay vào lại)
            canClaimToday = true;
        }
        else
        {
            currentDayIndex = 0;
            canClaimToday = true;
            Debug.Log("Đã reset về ngày 1 do đứt chuỗi hoặc lần đầu chơi");
        }
        if (currentDayIndex > 6)
        {
            currentDayIndex = 0;
        }
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
        if (level >= EconomyConfig.Instance.unitShop.increaseAfterLevel) UnitSpawner.Instance.LoadCost(saveData.costMelee, saveData.costRange);
        foreach (var m in saveData.dataMyTeam.units)
        {
            MonsterType tp = (MonsterType)Enum.Parse(typeof(MonsterType), m.type); ;
            GameObject obj = Instantiate(tp == MonsterType.Melee? meleePrefabs: rangePrefabs);
            MonsterHealth mh = obj.GetComponent<MonsterHealth>();
            mh.SetGridPos(m.gridX, m.gridY);
            mh.stats.type = tp;
            mh.LevelUp(m.level - 1);
            mh.stats.attackSpeed = m.attackSpeed;
            mh.stats.attackRange = m.attackRange;
            mh.stats.moveSpeed = m.moveSpeed;
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
    public bool SubCoins(long a) //Trừ coin của người chơi
    {
        if (a > coins)
        {
            Noti.Instance.Show("not_enough_gold");
            return false;
        }
        coins -= a;
        coins = Max(coins, 0);
        txtCoins.SetText(FormatMoney(coins));
        return true;
    }
    public void AddCoins(long a) //Thêm coin của người chơi
    {
        coins += a;
        coins = Min(coins, long.MaxValue);
        txtCoins.SetText(FormatMoney(coins));
    }
    public bool SubBooster(TypeBooster type) //Trừ coin của người chơi
    {
        if (type == TypeBooster.Freeze)
        {
            if (boosterFreeze <= 0)
            {
                BoosterManager.Instance.isOpenPanel = true;
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
        gems += a;
        gems = Mathf.Min(gems, int.MaxValue);
        txtGems.SetText(gems.ToString());
    }
}