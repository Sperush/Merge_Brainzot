using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LevelBgrManager : MonoBehaviour
{
    public Sprite[] BgrSprite;
    public Sprite[] unlockNewBackgroundSprite;
    public Sprite[] KhungSprite;
    public Image[] imgBgr;
    public Image[] img;
    public static LevelBgrManager Instance;
    public SpriteRenderer bgr;
    public Image bgrIcon;
    public SpriteRenderer khung;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Instance = this;
    }
    public void Load(bool isLoadGame)
    {
        int level = Char.Instance.level;
        int lvInPage = (level - 1) % 10;
        int page = (level - 1) / 10;
        for (int i = 0; i < 10; i++)
        {
            img[i].color = i < lvInPage ? Color.green: i == lvInPage ? Color.yellow : Color.white;
        }

        imgBgr[0].sprite = BgrSprite[page];
        imgBgr[1].sprite = BgrSprite[page + 1];
        bgr.sprite = BgrSprite[page];
        khung.sprite = KhungSprite[page];
        bgrIcon.sprite = BgrSprite[page];
        if(!isLoadGame) StartCoroutine(openPanel());
    }
    public IEnumerator openPanel()
    {
        yield return new WaitUntil(() => PanelManager.Instance.isOpenPanel == false);
        if ((Char.Instance.level - 1) % 10 == 0) PanelManager.Instance.OpenPanel(PanelManager.Instance.bgrPanel);
    }
}
