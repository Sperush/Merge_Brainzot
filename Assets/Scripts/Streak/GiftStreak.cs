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
    private void Start()
    {
        txt.SetText(milestone.ToString());
    }
    public void OpenGift()
    {
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
