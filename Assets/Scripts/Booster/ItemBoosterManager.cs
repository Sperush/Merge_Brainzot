using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemBoosterManager : MonoBehaviour
{
    public int price;
    public int amount;
    public TMP_Text txtAmount;
    public TMP_Text txtPrice;
    public Image img;
    public Sprite[] sp;
    bool isFreeze;

    public void Init(bool isFreeze)
    {
        txtAmount.SetText("+" + amount.ToString());
        txtPrice.SetText(price.ToString());
        img.sprite = sp[isFreeze ? 0 : 1];
        this.isFreeze = isFreeze;
    }

    public void Collect()
    {
        if(Char.Instance.SubGems(price))
        {
            Char.Instance.AddBooster(isFreeze ? TypeBooster.Freeze : TypeBooster.Bomp, amount);
        }
    }

}
