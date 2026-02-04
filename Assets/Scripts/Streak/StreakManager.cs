using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class StreakManager : MonoBehaviour
{
    public int maxStreak = 10;
    public Slider slider;
    public Slider sliderBgr;
    public TMP_Text txtCount;
    public static StreakManager Instance;
    public GiftStreak[] giftStreaks;
    public GameObject objRed;
    public int CurrentStep = 0;

    private void Awake() => Instance = this;

    public void Load()
    {
        int streak = Char.Instance.coutStreak;
        txtCount.SetText(streak.ToString());

        float val = CalculateLogicSliderValue();
        slider.maxValue = maxStreak;
        slider.value = val;

        foreach (var m in giftStreaks) m.Load();
        LoadRed();
    }
    public void OpenPanel()
    {
        Load();
        LoadBar();
        PanelManager.Instance.OpenPanel(PanelManager.Instance.streakPanel);
    }
    public void LoadBar()
    {
        sliderBgr.maxValue = maxStreak;
        sliderBgr.value = CalculateLogicSliderValue();
        LoadRed();
    }
    public bool IsActuallyInDebt()
    {
        int streak = Char.Instance.coutStreak;
        if (streak <= 10) return false;
        int correctCycle = (streak - 1) / 10;
        return Char.Instance.giftCycle < correctCycle;
    }
    public int GetCurrentStep()
    {
        int streak = Char.Instance.coutStreak;
        if (streak == 0) return 0;
        int step = streak % 10;
        return (step == 0) ? 10 : step;
    }
    private float CalculateLogicSliderValue()
    {
        int streak = Char.Instance.coutStreak;
        int currentStep = GetCurrentStep();
        if (IsActuallyInDebt()) return maxStreak;
        if (Char.Instance.giftCycle > (streak - 1) / 10) return 0;
        return currentStep;
    }

    public void LoadRed()
    {
        objRed.SetActive(isHaveGift());
    }

    public bool isHaveGift()
    {
        foreach (var m in giftStreaks)
        {
            if (!Char.Instance.giftCollected[m.id] && (m.milestone <= slider.value || m.milestone <= sliderBgr.value))
            {
                return true;
            }
        }
        return false;
    }

    public bool IsAllGiftsCollected()
    {
        for (int i = 0; i < giftStreaks.Length; i++)
        {
            if (i < Char.Instance.giftCollected.Count && !Char.Instance.giftCollected[i])
                return false;
        }
        return true;
    }

    public void resetStreak(bool isDone)
    {
        if (!isDone)
        {
            Char.Instance.giftCollected = new List<bool>() { false, false, false };
            Char.Instance.coutStreak = 0;
            Char.Instance.giftCycle = 0;
            Load();
            LoadBar();
        }
        else
        {
            Char.Instance.giftCycle++;
            StartCoroutine(WaitToResetGifts());
        }
    }

    IEnumerator WaitToResetGifts()
    {
        yield return new WaitForSeconds(1.5f);
        Char.Instance.giftCollected = new List<bool>() { false, false, false };
        Load();
        LoadBar();
    }
}