using UnityEngine;
using TMPro;
using DG.Tweening;
using System;

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

    public float regenTime = 300f; // 5 phút = 300 giây
    [Header("Fake Near Win")]
    [Range(0f, 1f)]
    public float fakeNearWinChance = 0.35f;

    bool isSpinning;

    private Quaternion initialRotation;

    int currentIndex = 0;
    float timerSpin;
    void Start()
    {
        UpdateUI();
        initialRotation = wheel.transform.rotation;
    }

    void Update()
    {
        timerSpin += Time.deltaTime;

        if (timerSpin >= regenTime)
        {
            timerSpin = 0f;
            Char.Instance.freeSpinLeft++;
            UpdateUI();
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
                UpdateUI();
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

        if (r.id == "no_reward")
        {
            Debug.Log("noo");
            txtOoop.text = $"Ooops!";
            txtCongratulation.text = $"";
            txtReward.text = $"Chúc bạn may mắn lần sau";
            txtResult.text = $"";
        }
        else
        {
            if (r.id == "coins")
            {
                Char.Instance.AddCoins(r.amount);
            }
            if (r.id == "gems")
            {
                Char.Instance.AddGems(r.amount);
            }
            Debug.Log("uia");
            txtOoop.text = $"";
            txtCongratulation.text = $"Congratulation!";
            txtReward.text = $"Nhấn vào màn hình để nhận thưởng";
            int value = r.amount;
            string text;
            if (value >= 1_000_000_000)
                text = (value / 1_000_000_000).ToString() + "B";
            else if (value >= 1_000_000)
                text = (value / 1_000_000).ToString() + "M";
            else if (value >= 1_000)
                text = (value / 1_000).ToString() + "K";
            else
                text = value.ToString();
            txtResult.text = $"+{text}";
            Debug.Log("Lucky Spin Reward: " + r.id);
        }

        luckySpinPanel.SetActive(false);
        luckySpinRewardPanel.SetActive(true);
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

    void UpdateUI()
    {
        //btnSpinFree.SetActive(freeSpinLeft > 0);
        //btnSpinAds.SetActive(freeSpinLeft <= 0);
    }

    public void CloseLuckySpinRewardPanel()
    {
        wheel.transform.rotation = initialRotation;
        luckySpinRewardPanel.SetActive(false);
        luckySpinPanel.SetActive(true);
        rewards[currentIndex].image.SetActive(false);
    }
}
