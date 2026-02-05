using UnityEngine;
using TMPro;
using DG.Tweening;
using System;
using System.Runtime.ConstrainedExecution;
using JetBrains.Annotations;

public class LuckySpinManager : MonoBehaviour
{
    [Header("Wheel")]
    public Transform wheel;
    public float spinDuration = 4f;

    [Header("Reward Config")]
    public SpinReward[] rewards;

    [Header("UI")]
    public TMP_Text txtResult;
    public TMP_Text txtCooldown;
    public TMP_Text txtReward;
    public TMP_Text txtOoop;
    public TMP_Text txtCongratulation;
    public TMP_Text txtSpinLeft;
    public GameObject btnSpinFree;
    public GameObject btnSpinAds;
    public GameObject luckySpinPanel;
    public GameObject luckySpinRewardPanel;
    public GameObject vfx;

    public float regenTime = 300f; // 5 phút = 300 giây
    [Header("Fake Near Win")]
    [Range(0f, 1f)]
    public float fakeNearWinChance = 0.35f;

    bool isSpinning;

    private Quaternion initialRotation;

    int currentIndex = 0;
    float timerSpin;
    string type;

    void Start()
    {
        UpdateUI();
        initialRotation = wheel.transform.rotation;
    }

    void Update()
    {
        timerSpin += Time.deltaTime;
        UpdateSpinLeft();
        if (timerSpin >= regenTime)
        {
            timerSpin = 0f;
            Char.Instance.freeSpinLeft++;
            //UpdateUI();
        }

        UpdateCooldownUI();
    }

    // ===================== SPIN =====================

    public void SpinFree()
    {
        if (isSpinning || Char.Instance.freeSpinLeft <= 0) return;

        Char.Instance.freeSpinLeft--;
        StartSpin();
    }

    /*public void SpinAds()
    {
        if (isSpinning) return;

        // 👉 GẮN ADS REWARD TẠI ĐÂY
        StartSpin();
    }*/

    void StartSpin()
    {
        PanelManager.Instance.BlockUI(true);
        isSpinning = true;
        txtResult.text = "";

        int rewardIndex = GetWeightedReward();
        currentIndex = rewardIndex;
        Debug.Log(rewardIndex);
        float targetAngle = GetTargetAngle(rewardIndex);

        wheel
            .DORotate(new Vector3(0, 0, targetAngle), spinDuration, RotateMode.FastBeyond360)
            .SetEase(Ease.OutQuart)
            .OnComplete(() =>
            {
                isSpinning = false;
                GiveReward(rewardIndex);
                UpdateSpinLeft();
            });
    }

    // ===================== REWARD LOGIC =====================

    int GetWeightedReward()
    {
        float total = 0;
        foreach (var r in rewards) total += r.weight;

        float rand = UnityEngine.Random.value * total + 0.1f;
        float cur = 0;

        for (int i = 0; i < rewards.Length; i++)
        {
            cur += rewards[i].weight;
            if (rand <= cur)
                return i;
        }
        return 0;
    }

    float GetTargetAngle(int index)
    {
        int count = rewards.Length;
        float anglePerItem = 360f / count;
        float extraRounds = 360f * UnityEngine.Random.Range(4, 6);

        //float angle = extraRounds + index * anglePerItem;
        float over = anglePerItem / 2.5f;
        float rand = (UnityEngine.Random.value > 0.5f) ? -over : over;


        float angle = extraRounds + index * anglePerItem + rand;
        return -angle;
    }

    void GiveReward(int index)
    {
        SpinReward r = rewards[index];

        Debug.Log("uia");
        txtOoop.text = $"";
        txtCongratulation.text = $"Congratulation!";
        txtReward.text = $"Nhấn vào màn hình để nhận thưởng";

        switch (r.id)
        {
            case "no_reward":
                Debug.Log("noo");
                txtOoop.text = $"Ooops!";
                txtCongratulation.text = $"";
                txtReward.text = $"Chúc bạn may mắn lần sau";
                txtResult.text = $"";
                break;
            case "coins":
                Char.Instance.AddCoins(r.amount);
                txtResult.text = $"+{SetCoinsText(r.amount)}";
                break;
            default:
                Char.Instance.AddGems(r.amount);
                txtResult.text = $"+{r.amount}";
                break;
        }

        Debug.Log("Lucky Spin Reward: " + r.id);
        luckySpinPanel.SetActive(false);
        PanelManager.Instance.isOpenPanel = false;
        PanelManager.Instance.OpenPanel(luckySpinRewardPanel);
        r.image.SetActive(true);
    }

    void UpdateCooldownUI()
    {
        txtSpinLeft.text = $"x{Char.Instance.freeSpinLeft}";

        float remain = regenTime - timerSpin;
        int min = Mathf.FloorToInt(remain / 60);
        int sec = Mathf.FloorToInt(remain % 60);
        if (Char.Instance.freeSpinLeft == 0)
        {
            txtCooldown.text = $"{min:D2}:{sec:D2}";
        }
        else
        {
            txtCooldown.text = $"";
        }
        
    }

    void UpdateSpinLeft()
    {
        if (Char.Instance.freeSpinLeft > 0)
        {
            vfx.SetActive(true);
        }
        else
        {
            vfx.SetActive(false);
        }
    }

    void UpdateUI()
    {
        for (int i = 0; i < rewards.Length; i++)
        {
            type = rewards[i].id;
            switch(type)
            {
                case "no_reward":
                    rewards[i].rewardText.SetText("No reward");
                    break;
                case "coins":
                    rewards[i].rewardText.SetText($"+{SetCoinsText(rewards[i].amount)}");
                    break;
                default:
                    rewards[i].rewardText.SetText($"+{rewards[i].amount}");
                    break;
            }
        }
    }

    string SetCoinsText(int value)
    {
        string text;
        if (value >= 1_000_000_000)
            text = (value / 1_000_000_000).ToString() + "B";
        else if (value >= 1_000_000)
            text = (value / 1_000_000).ToString() + "M";
        else if (value >= 1_000)
            text = (value / 1_000).ToString() + "K";
        else
            text = value.ToString();
        return text;
    }

    public void CloseLuckySpinRewardPanel()
    {
        wheel.transform.rotation = initialRotation;
        luckySpinPanel.SetActive(true);
        PanelManager.Instance.isOpenPanel = true;
        luckySpinRewardPanel.SetActive(false);
        rewards[currentIndex].image.SetActive(false);
    }
}
