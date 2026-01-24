using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PanelManager : MonoBehaviour
{
    public GameObject luckySpinPanel;
    public GameObject darkPanel;
    public GameObject dailyRewardPanel;

    public GameObject adsPanel;

    public GameObject collectionPanel;
    public GameObject rangeCollection;
    public GameObject meleeCollection;

    public GameObject settingPanel;

    public GameObject summonPanel;
    public GameObject rangeSummon;
    public GameObject meleeSummon;

    public GameObject unlockUnit;
    public GameObject giftPanel;
    public GameObject streakPanel;
    public GameObject bgrPanel;
    public ItemManager statsUnit;

    public GameObject moreGemsPanel;
    public GameObject BuyBoosterPanel;

    public static PanelManager Instance;

    [Header("Cài đặt")]
    public float duration = 0.5f; // Thời gian hiệu ứng
    public Ease openEase = Ease.OutBack; // Kiểu nảy khi mở
    public Ease closeEase = Ease.InBack; // Kiểu thu vào khi đóng

    private Vector3 initialScale;
    public ScrollRect[] scolls;
    public bool isOpenPanel;

    // Gọi hàm này để MỞ Panel
    public void OpenPanel(GameObject panel)
    {
        if (!isOpenPanel)
        {
            isOpenPanel = true;
            darkPanel.SetActive(true);
            if (panel == unlockUnit)
            {
                statsUnit.Load();
            }
            panel.SetActive(true);
            AudioManager.Instance.Play(GameSound.clickButtonSound);
            initialScale = new Vector3(1, 1, 1);
            panel.transform.localScale = Vector3.zero;
            panel.transform.DOScale(initialScale, duration).SetEase(openEase);
            ResetScroll();
        } else
        {
            StartCoroutine(Open(panel));
        }
    }
    public IEnumerator Open(GameObject panel)
    {
        yield return new WaitUntil(() => isOpenPanel == false);
        isOpenPanel = true;
        darkPanel.SetActive(true);
        if (panel == unlockUnit)
        {
            statsUnit.Load();
        }
        panel.SetActive(true);
        AudioManager.Instance.Play(GameSound.clickButtonSound);
        initialScale = new Vector3(1, 1, 1);
        panel.transform.localScale = Vector3.zero;
        panel.transform.DOScale(initialScale, duration).SetEase(openEase);
        ResetScroll();
    }
    public void ResetScroll()
    {
        Canvas.ForceUpdateCanvases(); // bắt buộc
        foreach(var m in scolls)
        {
            m.content.anchoredPosition = new Vector2(m.content.anchoredPosition.x, 0);
        }
    }

    // Gọi hàm này để ĐÓNG Panel
    public void ClosePanel(GameObject panel)
    {
        AudioManager.Instance.Play(GameSound.clickButtonSound);
        // Thu nhỏ về 0
        panel.transform.DOScale(Vector3.zero, duration) // Đóng thì nên nhanh hơn mở 1 chút
            .SetEase(closeEase)
            .OnComplete(() =>
            {
                isOpenPanel = false;
                // Sau khi thu nhỏ xong -> Tắt toàn bộ Container (biến mất cả nền đen)
                panel.SetActive(false);
                if(Char.Instance.level > 2 || BattleManager.Instance.startPvP) darkPanel.SetActive(false);
                BoosterManager.Instance.isOpenPanel = false;
            });
        if (TutorialController.Instance.currentState == TutorialController.TutorialState.Phase2_DragMerge)
        {
            TutorialController.Instance.tutorialCanvas.SetActive(true);
            TutorialController.Instance.SetState(TutorialController.TutorialState.Phase2_ClickBattle);
        }
    }

    private void Awake()
    {
        Instance = this;
    }

    public void showLuckySpinPanel()
    {
        if (luckySpinPanel != null)
        {
            OpenPanel(luckySpinPanel);
        }
    }
    public void hideLuckySpinPanel()
    {
        if (luckySpinPanel != null)
        {
            ClosePanel(luckySpinPanel);
        }
    }
    public void showNoAds()
    {
        if (adsPanel != null)
        {
            OpenPanel(adsPanel);
        }
    }
    public void hideNoAds()
    {
        if (adsPanel != null)
        {
            ClosePanel(adsPanel);
        }
    }

    public void showCollectionPanel()
    {
        if (collectionPanel != null)
        {
            OpenPanel(collectionPanel);
            rangeCollection.SetActive(true);
            meleeCollection.SetActive(false);
            UpdatePanelRange();
        }
    }
    public void UpdatePanelMelee()
    {
        foreach(var m in Char.Instance.itemMelee)
        {
            m.parent.SetActive(Char.Instance.unlockUnitMelee[m.level - 1]);
        }
    }
    public void UpdatePanelRange()
    {
        foreach (var m in Char.Instance.itemRange)
        {
            m.parent.SetActive(Char.Instance.unlockUnitRange[m.level - 1]);
        }
    }
    public void hideCollectionPanel()
    {
        if (collectionPanel != null)
        {
            ClosePanel(collectionPanel);
        }
    }
    public void showRangeCollection()
    {
        if (rangeCollection != null)
        {
            rangeCollection.SetActive(true);
            meleeCollection.SetActive(false);
            UpdatePanelRange();
            ResetScroll();
            AudioManager.Instance.Play(GameSound.clickButtonSound);
        }
    }
    public void showMeleeCollection()
    {
        if (rangeCollection != null)
        {
            rangeCollection.SetActive(false);
            meleeCollection.SetActive(true);
            UpdatePanelMelee();
            ResetScroll();
            AudioManager.Instance.Play(GameSound.clickButtonSound);
        }
    }

    public void showSettingPanel()
    {
        if (settingPanel != null)
        {
            OpenPanel(settingPanel);
        }
    }

    public void hideSettingPanel()
    {
        if (settingPanel != null)
        {
            ClosePanel(settingPanel);
        }
    }

    public void showSummonPanel()
    {
        if (summonPanel != null)
        {
            OpenPanel(summonPanel);
            rangeSummon.SetActive(true);
            meleeSummon.SetActive(false);
        }
    }
    public void hideSummonPanel()
    {
        if (summonPanel != null)
        {
            ClosePanel(summonPanel);
        }
    }
    public void showRangeSummon()
    {
        if (rangeSummon != null)
        {
            rangeSummon.SetActive(true);
            meleeSummon.SetActive(false);
            ResetScroll();
            AudioManager.Instance.Play(GameSound.clickButtonSound);
        }
    }
    public void showMeleeSummon()
    {
        if (rangeSummon != null)
        {
            rangeSummon.SetActive(false);
            meleeSummon.SetActive(true);
            ResetScroll();
            AudioManager.Instance.Play(GameSound.clickButtonSound);
        }
    }

    public void showMoreGemsPanel()
    {
        if (moreGemsPanel != null)
        {
            OpenPanel(moreGemsPanel);
        }
    }
    public void hideMoreGemsPanel()
    {
        if (moreGemsPanel != null)
        {
            ClosePanel(moreGemsPanel);
        }
    }
}
