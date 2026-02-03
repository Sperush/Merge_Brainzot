using UnityEngine;
using UnityEngine.EventSystems;
using System;
public enum ButtonType
{
    SpawnMelee,
    SpawnRange,
    Battle,
    Summon,
    Setting,
    Collection,
    LuckySpin,
    MoreGems,
    DailyReward,
    Gift,
    NoAds
}
public class Button2D : MonoBehaviour, IPointerClickHandler
{
    public ButtonType buttonType;
    public static event Action<ButtonType> OnButton2DClicked;
    public void OnPointerClick(PointerEventData eventData) //Kiểm tra va chạm raycast của Button
    {
        bool isSuccess = false;
        switch (buttonType)
        {
            case ButtonType.NoAds:
                if (BattleManager.Instance.startPvP) return;
                PanelManager.Instance.showNoAds();
                AudioManager.Instance.Play(GameSound.clickButtonSound);
                isSuccess = true;
                break;
            case ButtonType.Gift:
                if (BattleManager.Instance.startPvP) return;
                GiftManager.Instance.Load();
                PanelManager.Instance.OpenPanel(PanelManager.Instance.giftPanel);
                AudioManager.Instance.Play(GameSound.clickButtonSound);
                BattleManager.Instance.plane.speed = 0;
                isSuccess = true;
                break;
            case ButtonType.SpawnMelee:
                if (BattleManager.Instance.startPvP || GridManager.Instance.isFull() || (Char.Instance.level >= EconomyConfig.Instance.unitShop.increaseAfterLevel && !Char.Instance.SubCoins((int)UnitSpawner.Instance.costMelee))) return;
                UnitSpawner.Instance.SpawnMeleeUnit(0);
                isSuccess = true;
                break;
            case ButtonType.SpawnRange:
                if (BattleManager.Instance.startPvP! || GridManager.Instance.isFull() || (Char.Instance.level >= EconomyConfig.Instance.unitShop.increaseAfterLevel && !Char.Instance.SubCoins((int)UnitSpawner.Instance.costRange))) return;
                UnitSpawner.Instance.SpawnRangeUnit(0);
                isSuccess = true;
                break;
            case ButtonType.Battle:
                if (BattleManager.Instance.startPvP) return;
                BattleManager.Instance.StartBattle();
                AudioManager.Instance.Play(GameSound.fightSound);
                isSuccess = true;
                break;
            case ButtonType.LuckySpin:
                PanelManager.Instance.showLuckySpinPanel();
                AudioManager.Instance.Play(GameSound.clickButtonSound);
                isSuccess = true;
                break;
            case ButtonType.DailyReward:
                PanelManager.Instance.OpenPanel(PanelManager.Instance.dailyRewardPanel);
                AudioManager.Instance.Play(GameSound.clickButtonSound);
                isSuccess = true;
                break;
            case ButtonType.Collection:
                PanelManager.Instance.showCollectionPanel();
                AudioManager.Instance.Play(GameSound.clickButtonSound);
                isSuccess = true;
                break;
            case ButtonType.Setting:
                PanelManager.Instance.showSettingPanel();
                AudioManager.Instance.Play(GameSound.clickButtonSound);
                isSuccess = true;
                break;
            case ButtonType.Summon:
                PanelManager.Instance.showSummonPanel();
                AudioManager.Instance.Play(GameSound.clickButtonSound);
                isSuccess = true;
                break;
            case ButtonType.MoreGems:
                PanelManager.Instance.showMoreGemsPanel();
                AudioManager.Instance.Play(GameSound.clickButtonSound);
                isSuccess = true;
                break;

        }
        if (isSuccess)
        {
            OnButton2DClicked?.Invoke(buttonType);
        }
    }
}