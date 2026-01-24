using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System.IO;
using UnityEngine.UI;
using System.Collections;
public enum BattleState
{
    Idle,           // Đang nghỉ/Chờ
    Prepare,        // Đang giương cung/Múa (Animation)
    Firing,         // Đang sinh đạn
    WaitingForImpact// Chờ đạn bay trúng hết
}
public class BattleManager : MonoBehaviour
{
    public bool startPvP; //Kiểm tra xem có đang ở trạng thái Fight không
    public List<GameObject> playerTeam = new List<GameObject>();
    public List<GameObject> enemyTeam = new List<GameObject>();
    public List<GameObject> arrUnitReady = new List<GameObject>();
    public List<MonsterAI> arrRange = new List<MonsterAI>();
    public static BattleManager Instance;
    public GameObject meleeEnemyPrefabs;
    public GameObject rangeEnemyPrefabs;
    public GameObject winPanel;
    public GameObject losePanel;
    public GameObject ButtonList;
    public TMP_Text[] txtCoinReward;
    public TMP_Text txtGemReward;
    public GameObject rangeEnemyPrefab;
    public GameObject meleeEnemyPrefab;
    public GameObject Booster;
    public Button[] btnReward;
    [Header("Config")]
    public float animationDelay = 0.4f; // Thời gian chờ animation đánh (tay vung lên)
    public float reloadTime = 0.8f;     // Thời gian nghỉ giữa các đợt
    public float maxWaitTime = 5.0f;    // [QUAN TRỌNG] Thời gian chờ tối đa (để chống kẹt đạn)
    [Header("Status")]
    public BattleState currentState = BattleState.Idle;
    public int activeBulletCount = 0;
    public BombPlane plane;
    public GameObject[] buttonGift;
    private float _levelStartTime;
    private long lastCoinReward;
    private float _stateTimer;
    private void Awake()
    {
        Instance = this;
        btnReward[0].onClick.AddListener(ShowRewardedFromButton);
        btnReward[1].onClick.AddListener(ShowRewardedFromButton);
    }
    public bool isOkPvP()
    {
        return arrUnitReady.Count == playerTeam.Count + enemyTeam.Count;
    }
    void Update()
    {
        if (startPvP && isOkPvP() && !CheckBattleEnd() && !BoosterManager.Instance.isOpenPanel)
        {
            switch (currentState)
            {
                case BattleState.Idle:
                    // Hết thời gian nghỉ -> Chuyển sang chuẩn bị bắn
                    _stateTimer -= Time.deltaTime;
                    if (_stateTimer <= 0)
                    {
                        StartCoroutine(AttackRoutine());
                    }
                    break;

                case BattleState.WaitingForImpact:
                    // Logic chờ đạn:
                    // 1. Nếu đạn đã hết (activeBulletCount == 0) -> Xong
                    // 2. HOẶC chờ quá lâu (stateTimer < 0) -> Xong (Force qua lượt mới để ko bị lag game)

                    _stateTimer -= Time.deltaTime;
                    if (activeBulletCount <= 0 || _stateTimer <= 0)
                    {
                        // Về lại trạng thái nghỉ để nạp đạn
                        currentState = BattleState.Idle;
                        _stateTimer = reloadTime;
                    }
                    break;
            }
        }
    }
    IEnumerator AttackRoutine()
    {
        // GIAI ĐOẠN 1: PREPARE (Múa)
        currentState = BattleState.Prepare;
        //foreach (var unit in arrRange)
        //{
        //    if (unit != null && unit.gameObject.activeSelf) unit.PlayAttackAnimation(); // Hàm chỉ chạy anim
        //}

        yield return new WaitForSeconds(animationDelay);
        // GIAI ĐOẠN 2: FIRING (Bắn)
        currentState = BattleState.Firing;
        // Reset đếm đạn về 0 để bắt đầu đếm đợt mới
        activeBulletCount = 0;
        int shootersCount = 0;
        for (int i = arrRange.Count - 1; i >= 0; i--)
        {
            MonsterAI unit = arrRange[i];
            if (unit != null && unit.gameObject.activeSelf && !unit.isFrozen && unit.HasTarget() && unit.projectile == null)
            {
                unit.ForceAttack(); // Bắn
                shootersCount++;
            }
        }
        // GIAI ĐOẠN 3: WAITING (Chờ đạn bay)
        // Nếu không có ai bắn (do chết hết hoặc ko có target) thì nghỉ luôn
        if (shootersCount == 0)
        {
            currentState = BattleState.Idle;
            _stateTimer = 0.5f;
        }
        else
        {
            currentState = BattleState.WaitingForImpact;
            // Đặt thời gian chờ tối đa (Safety Net)
            _stateTimer = maxWaitTime;
        }
    }
    public float GetDifficulty(int level)
    {
        var cfg = BattleConfig.Instance.difficulty;
        var manual = cfg.manualLevels.ToDictionary();

        if (manual.ContainsKey(level))
            return manual[level];

        int groupIndex = (level - 11) / 5;
        int indexInGroup = (level - 11) % 5;

        return cfg.baseGroup[indexInGroup] + groupIndex * cfg.groupIncrease;
    }
    public long CalulatorReward(bool isWin)
    {
        var rewardCfg = BattleConfig.Instance.reward;
        double reward = (UnitSpawner.Instance.costMelee + UnitSpawner.Instance.costRange) / 2.0 * (isWin ? rewardCfg.winMultiplier : rewardCfg.loseMultiplier) * GetDifficulty(Char.Instance.level);
        ////FAIL SAFE REWARD
        //if (!isWin && LoseTracker.IsFailSafeActive())
        //{
        //    reward *= FailSafeConfig.Instance.failSafe.rewardBonus;
        //}
        lastCoinReward = (long)System.Math.Round(reward);
        return lastCoinReward;
    }
    public void ShowRewardedFromButton()
    {
        //RewardedAds.Instance.LoadRewardedAd((isSuccess) =>
        //{
        //    if (isSuccess)
        //    {
        //        Char.Instance.AddCoins(lastCoinReward * AdsConfig.Instance.adsConfig.RewardMultiplier);
        //        Debug.Log("Đã cộng tiền thành công!");
        //    }
        //    else
        //    {
        //        Debug.Log("Người chơi tắt ngang hoặc lỗi Ad, không thưởng.");
        //    }
        //});
    }
    public bool CheckBattleEnd() //Kiểm tra xem team nào thắng team nào thua
    {
        if (!enemyTeam.Exists(m => m.activeSelf))
        {
            GameLog.Log("battle_win", new
            {
                level = Char.Instance.level,
                loseStreak = LoseTracker.loseStreak,
                mergeCount = MergeTracker.mergeCount
            });
            LoseTracker.OnWin();
            Debug.Log("Player Win");
            AudioManager.Instance.Play(GameSound.victorySound);
            Char.Instance.level++;
            Char.Instance.AddStreakBar(1);
            EndGame(true);
            VFXManager.Instance.Play(VFXType.WinFirework, winPanel.transform.localPosition);
            if (Char.Instance.level <= 2) Char.Instance.Save(Application.persistentDataPath + "/save.json");
            return true;
        }
        else if (!playerTeam.Exists(m => m.activeSelf))
        {
            GameLog.Log("battle_lose", new
            {
                level = Char.Instance.level,
                loseStreak = LoseTracker.loseStreak
            });
            LoseTracker.OnLose();
            Debug.Log("Enemy Win");
            Char.Instance.AddStreakBar(-Char.Instance.coutStreak);
            AudioManager.Instance.Play(GameSound.loseSound);
            EndGame(false);
            return true;
        }
        return false;
    }
    public void EndGame(bool isWin)
    {
        long gem = Char.Instance.level % 5 == 0 ? Char.Instance.level/5:0;
        long coin = CalulatorReward(isWin);
        txtCoinReward[isWin ? 0 : 1].SetText(Char.FormatMoney(coin));
        if(isWin) txtGemReward.SetText(Char.FormatMoney(gem));
        Char.Instance.AddCoins(coin);
        startPvP = false;
        ButtonList.SetActive(true);
        Booster.SetActive(false);
        PanelManager.Instance.streakPanel.SetActive(false);
        PanelManager.Instance.isOpenPanel = false;
        PanelManager.Instance.OpenPanel(isWin ? winPanel : losePanel);
        float duration = Time.time - _levelStartTime;
        buttonGift[0].SetActive(false);
        buttonGift[1].SetActive(false);
        if (Char.Instance.level > 3 && AdsConfig.Instance.adsConfig.InterEnable && (!isWin || duration > AdsConfig.Instance.adsConfig.MinGameplaySec))
        {
            //StartCoroutine(InterstitialAds.Instance.ShowAdsOnStart());
        }
        //Time.timeScale = 0f;
    }
    public void resetlevel() //Thua nên bấm nút sẽ chơi lại màn đấy
    {
        GridManager.Instance.CLear(4,5);
        arrUnitReady.Clear();
        foreach (var m in enemyTeam)
        {
            m.SetActive(true);
            MonsterAI ai = m.GetComponent<MonsterAI>();
            ai.enabled = false;
            ai.isReady = false;
            Destroy(ai.projectile);
            m.GetComponent<MonsterHealth>().ResetStatus();
        }
        foreach (var m in playerTeam)
        {
            m.SetActive(true);
            MonsterAI ai = m.GetComponent<MonsterAI>();
            ai.enabled = false;
            ai.isReady = false;
            Destroy(ai.projectile);
            m.GetComponent<MonsterHealth>().ResetStatus();
        }
        PanelManager.Instance.ClosePanel(losePanel);
        plane.Init(() =>{}, true);
        AudioManager.Instance.Play(GameSound.coinSound);
    }
    public void StartBattle() //Bắt đầu Fight
    {
        playerTeam.RemoveAll(m => m == null);
        if (!playerTeam.Exists(m => m.activeSelf)) return;
        foreach (var m in playerTeam)
        {
            if (m == null) continue;
            m.GetComponent<MonsterAI>().enabled = true;
        }
        foreach (var m in enemyTeam)
        {
            if (m == null) continue;
            m.GetComponent<MonsterAI>().enabled = true;
        }
        startPvP = true;
        ButtonList.SetActive(false);
        if(Char.Instance.level > 2) Booster.SetActive(true);
        plane.gameObject.SetActive(false);
    }
    public void ChangeLevelUp() //Thắng nên bấm nút sẽ chuyển tới level tiếp theo
    {
        MergeTracker.Reset();
        GridManager.Instance.CLear(4,5);
        arrUnitReady.Clear();
        foreach (var m in playerTeam)
        {
            m.SetActive(true);
            MonsterAI ai = m.GetComponent<MonsterAI>();
            ai.enabled = false;
            ai.isReady = false;
            Destroy(ai.projectile);
            m.GetComponent<MonsterHealth>().ResetStatus();
        }
        LoadLevel(false);
        PanelManager.Instance.ClosePanel(winPanel);
        AudioManager.Instance.Play(GameSound.coinSound);
        if (Char.Instance.level == 2) {
            TutorialController.Instance.StartPhase2_Merge();
            UnitSpawner.Instance.OnCost();
        }
    }

    //public void GenerateEnemy()
    //{
    //    GameObject obj;
    //    MonsterHealth mh;

    //    for (int i = 5; i >= 3; i--)
    //    {
    //        int sl = Random.Range(1, 4);
    //        List<int> arr = new List<int> { 0, 1, 2, 3, 4 };
    //        for (int j = 0; j < sl; j++)
    //        {
    //            obj = Instantiate(i == 5 ? rangeEnemyPrefabs : meleeEnemyPrefabs);
    //            mh = obj.GetComponent<MonsterHealth>();
    //            mh.LevelUp(Random.Range(1, 3));
    //            enemyTeam.Add(obj);
    //            int x = arr[Random.Range(0, arr.Count)];
    //            arr.Remove(x);
    //            GridManager.Instance.Place(mh, x, i);
    //        }
    //    }
    //}
    IEnumerator CheckDailyReward()
    {
        yield return new WaitUntil(() => PanelManager.Instance.isOpenPanel == false);
        if (Char.Instance.canClaimToday && Char.Instance.level > 2)
        {
            PanelManager.Instance.OpenPanel(PanelManager.Instance.dailyRewardPanel);
        }
    }
    public void LoadLevel(bool isLoadGame)
    {
        StartCoroutine(CheckDailyReward());
        DangerWarning.Instance.Show((Char.Instance.level > 9 && Char.Instance.level % 5 == 0) ? TypeDanger.VeryHard : (Char.Instance.level > 9 && (Char.Instance.level + 1) % 5 == 0) ? TypeDanger.Hard : TypeDanger.Normal);
        LevelBgrManager.Instance.Load(isLoadGame);
        _levelStartTime = Time.time;
        GridManager grid = GridManager.Instance;

        // Clear enemy cũ
        grid.CLearEnemy(4, 3);
        foreach (var m in enemyTeam)
        {
            if (m == null) continue;
            MonsterAI ai = m.GetComponent<MonsterAI>();
            Destroy(ai.projectile);
            Destroy(m.gameObject);
        }
        enemyTeam.Clear();
        Char.Instance.txtLevel.SetText(Noti.Get("level_format", Char.Instance.level));
        // Load JSON từ Resources
        string path = "Level/" + Char.Instance.level;
        TextAsset jsonFile = Resources.Load<TextAsset>(path);

        if (jsonFile == null)
        {
            Debug.LogError($"❌ Không tìm thấy file level: Resources/{path}.json");
            return;
        }

        DataSave dataSave = JsonUtility.FromJson<DataSave>(jsonFile.text);

        foreach (var m in dataSave.enemyTeam.units)
        {
            GameObject prefab = m.type == MonsterType.Melee.ToString() ? meleeEnemyPrefab : rangeEnemyPrefab;
            GameObject obj = Instantiate(prefab);
            MonsterHealth mh = obj.GetComponent<MonsterHealth>();
            mh.stats.type = (MonsterType)System.Enum.Parse(typeof(MonsterType), m.type);
            mh.SetGridPos(m.gridX, m.gridY);
            mh.LevelUp(m.level - 1);
            grid.Place(mh, mh.gridX, mh.gridY);
            enemyTeam.Add(obj);
        }
    }

}
