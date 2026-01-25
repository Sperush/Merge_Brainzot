using TMPro;
using UnityEngine;
using UnityEngine.UI;
[System.Serializable]
public class ItemGift
{
    public Item item;
    public int quantity;
    public Sprite img;
}
[System.Serializable]
public enum Item
{
    gem,
    gold,
    freeze,
    bomp
}
public class GiftStreak : MonoBehaviour
{
    public int id;
    public int milestone;
    public ItemGift reward;
    public TMP_Text txt;
    public Button btn;
    public Image img;
    public Sprite[] sp;
    private void Start()
    {
        txt.SetText(milestone.ToString());
        img.sprite = sp[Char.Instance.giftCollected[id] ? 1 : 0];
    }
    public void OpenGift()
    {
        if (Char.Instance.giftCollected[id] || Char.Instance.coutStreak < milestone) return;
        switch (reward.item)
        {
            case Item.gem:
                Char.Instance.AddGems(reward.quantity);
                break;
            case Item.gold:
                Char.Instance.AddCoins(reward.quantity);
                break;
            case Item.bomp:
                Char.Instance.AddBooster(TypeBooster.Bomp, reward.quantity);
                break;
            case Item.freeze:
                Char.Instance.AddBooster(TypeBooster.Freeze, reward.quantity);
                break;
        }
        Char.Instance.giftCollected[id] = true;
        img.sprite = sp[1];
        if (!Char.Instance.giftCollected.Contains(false)) StreakManager.Instance.resetStreak(true);
        StreakManager.Instance.LoadRed();
    }
    public void Load()
    {
        img.sprite = sp[Char.Instance.giftCollected[id] ? 1:0];
    }
}
