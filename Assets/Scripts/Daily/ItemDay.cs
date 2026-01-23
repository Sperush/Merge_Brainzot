using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public enum RewardState
{
    Locked,       // Chưa đến ngày (Ngày tương lai)
    ReadyToClaim, // Đến ngày nhận (Hôm nay)
    Claimed       // Đã nhận rồi
}
public class ItemDay : MonoBehaviour
{
    public int day;
    public RewardState rewardItem;
    public ItemGift[] reward;
    [Header("UI")]
    public Image img;
    public Image[] iconItem;
    public TMP_Text dayText;
    public TMP_Text[] rewardAmountText;
    [Header("State Objects")]
    public Sprite[] stateObj; //0:active 1:claim 2:locked
    private void Awake()
    {
        LanguageDropdown.days.Add(this);
    }
    private void Start()
    {
        UpdateUI();
    }
    public void SetState(RewardState state, int dayIndex)
    {
        dayText.SetText(Noti.Get("day_format", dayIndex == 6 ? 7: dayIndex + 1));
        int leg = reward.Length;
        for (int i = 0; i < leg; i++)
        {
            iconItem[i].sprite = reward[i].img;
            rewardAmountText[i].text = reward[i].quantity.ToString();
            switch (state)
            {
                case RewardState.Locked:
                    img.sprite = stateObj[2];
                    break;
                case RewardState.ReadyToClaim:
                    img.sprite = stateObj[0];
                    break;
                case RewardState.Claimed:
                    img.sprite = stateObj[1];
                    break;
            }
        }
    }
    private void UpdateUI()
    {
        RewardState state;
        if (day < Char.Instance.currentDayIndex)
        {
            // Những ngày trước đó -> Đã nhận
            state = RewardState.Claimed;
        }
        else if (day == Char.Instance.currentDayIndex)
        {
            // Ngày hiện tại
            if (Char.Instance.canClaimToday) state = RewardState.ReadyToClaim;
            else state = RewardState.Claimed;
        }
        else
        {
            // Những ngày tương lai -> Khóa
            state = RewardState.Locked;
        }
        if (day < 7) SetState(state, day);
    }
    public void OnClaimButtonPressed()
    {
        if (!Char.Instance.canClaimToday || day != Char.Instance.currentDayIndex) return;
        int leg = reward.Length;
        for (int i = 0; i < leg; i++)
        {
            switch (reward[i].item)
            {
                case Item.gem:
                    Char.Instance.AddGems(reward[i].quantity);
                    break;
                case Item.gold:
                    Char.Instance.AddCoins(reward[i].quantity);
                    break;
                case Item.bomp:
                    Char.Instance.AddBooster(TypeBooster.Bomp, reward[i].quantity);
                    break;
                case Item.freeze:
                    Char.Instance.AddBooster(TypeBooster.Freeze, reward[i].quantity);
                    break;
            }
        }
        Char.Instance.LastClaimTime = DateTime.Now.Date.ToString();
        //Char.Instance.currentDayIndex++;
        Char.Instance.canClaimToday = false;
        UpdateUI();
    }
}
