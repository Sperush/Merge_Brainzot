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
    Firing         // Đang sinh đạn
}
public class BattleManager : MonoBehaviour
{
    public bool startPvP; //Kiểm tra xem có đang ở trạng thái Fight không
    public List<GameObject> playerTeam = new List<GameObject>();
    public List<GameObject> enemyTeam = new List<GameObject>();
    public List<GameObject> arrUnitReady = new List<GameObject>();
    public List<MonsterAI> arrRange = new List<MonsterAI>();
    public static BattleManager Instance;
    public GameObject winPanel;
    public GameObject losePanel;
    public GameObject ButtonList;
    public TMP_Text[] txtCoinReward;
    public TMP_Text txtGemReward;
    public GameObject[] rangeEnemyPrefab;
    public GameObject[] meleeEnemyPrefab;
    public GameObject Booster;
    public Button[] btnReward;
    [Header("Config")]
    public float reloadTime = 0.8f;     // Thời gian nghỉ giữa các đợt
    public float maxWaitTime = 5.0f;    // [QUAN TRỌNG] Thời gian chờ tối đa (để chống kẹt đạn)
    [Header("Status")]
    public BattleState currentState = BattleState.Idle;
    public BombPlane plane;
    public GameObject buttonGift;
    private float _levelStartTime;
    private long lastCoinReward;
    private float _stateTimer;
    public long coinReward = 0;
    public int gemReward = 0;
    public GameObject[] gemWin;

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
            // Hết thời gian nghỉ -> Chuyển sang chuẩn bị bắn
            _stateTimer -= Time.deltaTime;
            if (_stateTimer <= 0)
            {
                foreach (var unit in arrRange)
                {
                    if (unit != null && unit.gameObject.activeSelf && unit.canAttack()) unit.PlayAttackAnimation(); // Hàm chỉ chạy anim
                }
                _stateTimer = BattleConfig.Instance.AttackRangeSpeed;
            }
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
    public float GetDifficultyMul(int level)
    {
        if (level % 5 == 0) return 1.4f;
        if ((level + 1) % 5 == 0) return 1.1f;
        return 1f;
    }
    public long CalulatorReward(bool isWin)
    {
        var rewardCfg = BattleConfig.Instance.reward;
        double reward = (UnitSpawner.Instance.costMelee + UnitSpawner.Instance.costRange) / 2.0 * (isWin ? rewardCfg.winMultiplier : rewardCfg.loseMultiplier) * GetDifficultyMul(Char.Instance.level);
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
        if (!playerTeam.Exists(m => m.activeSelf))
        {
            GameLog.Log("battle_lose", new
            {
                level = Char.Instance.level,
                loseStreak = LoseTracker.loseStreak
            });
            LoseTracker.OnLose();
            Debug.Log("Enemy Win");
            StreakManager.Instance.resetStreak(false);
            AudioManager.Instance.Play(GameSound.loseSound);
            EndGame(false);
            return true;
        } else if (!enemyTeam.Exists(m => m.activeSelf))
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
            Char.Instance.AddStreakBar(1);
            EndGame(true);
            VFXManager.Instance.Play(VFXType.WinFirework, winPanel.transform.localPosition);
            if (Char.Instance.level <= 2) Char.Instance.Save(Application.persistentDataPath + "/save.json");
            return true;
        }
        return false;
    }
    public void EndGame(bool isWin)
    {
        bool isCan = Char.Instance.level % 5 == 0;
        gemReward = isCan ? Char.Instance.level / 5:0;
        coinReward = CalulatorReward(isWin);
        txtGemReward.SetText(Char.FormatMoney(gemReward));
        if (isWin)
        {
            if (isCan)
            {
                gemWin[0].SetActive(true);
                gemWin[1].SetActive(true);
            } else
            {
                gemWin[0].SetActive(false);
                gemWin[1].SetActive(false);
            }
            Char.Instance.level++;
        }
        txtCoinReward[isWin ? 0 : 1].SetText(Char.FormatMoney(coinReward));
        startPvP = false;
        ButtonList.SetActive(true);
        Booster.SetActive(false);
        PanelManager.Instance.streakPanel.SetActive(false);
        PanelManager.Instance.isOpenPanel = false;
        PanelManager.Instance.OpenPanel(isWin ? winPanel : losePanel);
        float duration = Time.time - _levelStartTime;
        buttonGift.SetActive(false);
        if (Char.Instance.level > 3 && AdsConfig.Instance.adsConfig.InterEnable && (!isWin || duration > AdsConfig.Instance.adsConfig.MinGameplaySec))
        {
            //StartCoroutine(InterstitialAds.Instance.ShowAdsOnStart());
        }
    }
    public void resetlevel() //Thua nên bấm nút sẽ chơi lại màn đấy
    {
        Char.Instance.AddCoins(coinReward);
        ResetLV();
    }
    public void ResetLV()
    {
        GridManager.Instance.CLear(4, 2);
        foreach (var m in playerTeam)
        {
            if (m == null) continue;
            SafeResetUnit(m);
        }
        LoadLevel(false);
        PanelManager.Instance.ClosePanel(losePanel);
        plane.Init(() => { }, true);
        AudioManager.Instance.Play(GameSound.coinSound);
    }
    public void SafeResetUnit(GameObject m, bool isDel=false)
    {
        StartCoroutine(SafeResetUnitDelayed(m, isDel));
    }
    IEnumerator SafeResetUnitDelayed(GameObject m, bool isDel)
    {
        if (m == null) yield break;

        var ai = m.GetComponent<MonsterAI>();
        var anim = ai.animator;
        var skins = m.GetComponentsInChildren<UnityEngine.U2D.Animation.SpriteSkin>();
        ai.enabled = false;

        // 🛑 1. STOP ALL JOB SOURCES
        if (anim != null)
            anim.enabled = false;

        foreach (var s in skins)
            if (s != null)
                s.enabled = false;

        // 🕒 2. CHỜ JOB FLUSH
        yield return null;
        if (!isDel && !m.activeSelf) m.SetActive(true);
        // 🧹 3. CLEAN LOGIC
        ai.ResetAIState();

        if (!isDel)
            ai.monsterHealth.ResetStatus();

        if (ai.projectile != null)
            Destroy(ai.projectile);

        // 🧨 4A. DESTROY FLOW
        if (isDel)
        {
            Destroy(m);
            yield break;
        }

        // ♻️ 4B. RESET / POOL FLOW
        foreach (var s in skins)
            if (s != null)
                s.enabled = true;

        anim.enabled = true;
        anim.Rebind();
        anim.Update(0f);
    }

    public void StartBattle() //Bắt đầu Fight
    {
        arrUnitReady.Clear();
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
        Char.Instance.AddCoins(coinReward);
        Char.Instance.AddGems(gemReward);
        ChangeLV();
    }
    public void ChangeLV()
    {
        MergeTracker.Reset();
        GridManager.Instance.CLear(4, 2);
        foreach (var m in playerTeam)
        {
            if (m == null) continue;
            SafeResetUnit(m);
        }
        LoadLevel(false);
        PanelManager.Instance.ClosePanel(winPanel);
        AudioManager.Instance.Play(GameSound.coinSound);
        if (Char.Instance.level == 2)
        {
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
    public void CheckDailyReward()
    {
        if (PanelManager.Instance.isOpenPanel) StartCoroutine(Check());
        else if (Char.Instance.canClaimToday && Char.Instance.level > 2)
        {
            PanelManager.Instance.OpenPanel(PanelManager.Instance.dailyRewardPanel);
        }
    }
    IEnumerator Check()
    {
        yield return new WaitUntil(() => PanelManager.Instance.isOpenPanel == false);
        if (Char.Instance.canClaimToday && Char.Instance.level > 2)
        {
            PanelManager.Instance.OpenPanel(PanelManager.Instance.dailyRewardPanel);
        }
    }
    public void LoadLevel(bool isLoadGame)
    {
        StartCoroutine(LoadLevelRoutine(isLoadGame));
    }

    IEnumerator LoadLevelRoutine(bool isLoadGame)
    {
        CheckDailyReward();
        UnitSpawner.Instance.OnCost();
        DangerWarning.Instance.Show((Char.Instance.level > 9 && Char.Instance.level % 5 == 0) ? TypeDanger.VeryHard : (Char.Instance.level > 9 && (Char.Instance.level + 1) % 5 == 0) ? TypeDanger.Hard : TypeDanger.Normal);
        LevelBgrManager.Instance.Load(isLoadGame);
        _levelStartTime = Time.time;

        // 1️⃣ CLEAR GRID

        // 2️⃣ RESET ENEMY CŨ (AN TOÀN)
        foreach (var m in enemyTeam)
        {
            if (m == null) continue;
            SafeResetUnit(m, true);
        }
        GridManager.Instance.CLearEnemy(4,3);
        enemyTeam.Clear();

        // ⏱️ CHỜ JOB XƯƠNG + DESTROY XONG
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        // 3️⃣ LOAD JSON
        Char.Instance.txtLevel.SetText(
            Noti.Get("level_format", Char.Instance.level)
        );

        string path = "Level/" + Char.Instance.level;
        TextAsset jsonFile = Resources.Load<TextAsset>(path);

        if (jsonFile == null)
        {
            Debug.LogError($"❌ Không tìm thấy file level: Resources/{path}.json");
            yield break;
        }
        SpawnEnemy(JsonUtility.FromJson<DataSave>(jsonFile.text));
        // 4️⃣ SPAWN ENEMY MỚI
    }
    public void SpawnEnemy(DataSave dataSave)
    {
        foreach (var m in dataSave.enemyTeam.units)
        {
            GameObject prefab = GetUnitPrefabs(
                m.level,
                m.type == MonsterType.Melee.ToString()
            );

            GameObject obj = Instantiate(prefab);

            MonsterHealth mh = obj.GetComponent<MonsterHealth>();
            mh.SetGridPos(m.gridX, m.gridY);
            mh.SetStats(m.level);

            GridManager.Instance.Place(mh, mh.gridX, mh.gridY);
            enemyTeam.Add(obj);
        }
    }

    public GameObject GetUnitPrefabs(int level, bool isMelee)
    {
        var arr = isMelee ? meleeEnemyPrefab : rangeEnemyPrefab;
        level = Mathf.Clamp(level - 1, 0, arr.Length - 1);
        return arr[level];
    }

}
