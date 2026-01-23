using TMPro;
using UnityEngine;
using UnityEngine.UI;
[System.Serializable]
public class ItemGift
{
    public int gem;
    public int gold;
    public int booster1;
    public int booster2;
}
public class GiftStreak : MonoBehaviour
{
    public int id;
    public int milestone;
    public ItemGift reward;
    public TMP_Text txt;
    public Button btn;
    private void Start()
    {
        txt.SetText(milestone.ToString());
    }
    public void OpenGift()
    {
        Char.Instance.AddGems(reward.gem);
        Char.Instance.AddCoins(reward.gold);
        //Add reward booster
        btn.interactable = false;
        Char.Instance.giftCollected[id] = true;
        if (!Char.Instance.giftCollected.Contains(false)) StreakManager.Instance.resetStreak();
        StreakManager.Instance.LoadRed();
    }
    public void Load()
    {
        btn.interactable = Char.Instance.coutStreak >= milestone && !Char.Instance.giftCollected[id];
    }
}
