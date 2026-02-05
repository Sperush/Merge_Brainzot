using TMPro;
using UnityEngine;

public class GiftManager : MonoBehaviour
{
    public TMP_Text txtCoin;
    private long coin = 0;
    public GameObject giftbutton;
    public static GiftManager Instance;
    public void Start()
    {
        Instance = this;
        Load();
    }
    public void Load()
    {
        if (coin <= 0) coin = (int)(BattleManager.Instance.CalulatorReward(false) * Random.Range(1f, 2f));
        txtCoin.SetText(Char.FormatMoney(coin));
    }
    public void onCollect()
    {
        RewardedAds.Instance.LoadRewardedAd((isSuccess) =>
        {
            if (isSuccess)
            {
                Char.Instance.AddCoins(coin * AdsConfig.Instance.adsConfig.RewardMultiplier);
                Debug.Log("Đã cộng tiền thành công!");
                giftbutton.SetActive(false);
                BattleManager.Instance.plane.gameObject.SetActive(false);
                coin = 0;
                closePanel();
            }
            else
            {
                Debug.Log("Người chơi tắt ngang hoặc lỗi Ad, không thưởng.");
            }
        });
    }
    public void closePanel()
    {
        PanelManager.Instance.ClosePanel(PanelManager.Instance.giftPanel);
        BattleManager.Instance.plane.speed = 2;
    }
}
