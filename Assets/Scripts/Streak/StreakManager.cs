using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class StreakManager : MonoBehaviour
{
    public int maxStreak = 7;
    public Slider slider;
    public Slider sliderBgr;
    public TMP_Text txtCount;
    public static StreakManager Instance;
    public GiftStreak[] giftStreaks;
    public GameObject objRed;
    public void Start()
    {
        Instance = this;
    }
    public void Load()
    {
        slider.maxValue = maxStreak;
        txtCount.SetText(Char.Instance.coutStreak.ToString());
        slider.value = Mathf.Min(Char.Instance.coutStreak, maxStreak);
        foreach (var m in giftStreaks){
            m.Load();
        }
    }
    public void OpenPanel()
    {
        Load();
        PanelManager.Instance.OpenPanel(PanelManager.Instance.streakPanel);
    }
    public void LoadBar()
    {
        LoadRed();
        sliderBgr.maxValue = maxStreak;
        sliderBgr.value = Mathf.Min(Char.Instance.coutStreak, maxStreak);
    }
    public void resetStreak(bool isDone)
    {
        if (!isDone)
        {
            resetNow(false);
        } else
        {
            StartCoroutine(startReset());
        }
    }
    IEnumerator startReset()
    {
        yield return new WaitForSeconds(2f);
        resetNow(true);
    }
    public void resetNow(bool isDone)
    {
        Char.Instance.giftCollected = new List<bool>() { false, false, false };
        Char.Instance.coutStreak = isDone ? Char.Instance.coutStreak%10:0;
        LoadBar();
        Load();
    }
    public void LoadRed()
    {
        if (isHaveGift()) objRed.SetActive(true);
        else objRed.SetActive(false);
    }
    public bool isHaveGift()
    {
        foreach (var m in giftStreaks)
        {
            if (m.milestone <= Char.Instance.coutStreak && !Char.Instance.giftCollected[m.id]) return true;
        }
        return false;
    }
}