using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LevelBgrManager : MonoBehaviour
{
    public Sprite[] BgrSprite;
    public Image[] imgBgr;
    public Image[] img;
    public static LevelBgrManager Instance;
    public SpriteRenderer bgr;
    public Image bgrIcon;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Instance = this;
    }
    public IEnumerator Load(bool isLoadGame)
    {
        yield return new WaitForSeconds(2f);
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
        bgrIcon.sprite = BgrSprite[page];
        if(!isLoadGame && (level - 1) % 10 == 0) PanelManager.Instance.OpenPanel(PanelManager.Instance.bgrPanel);
    }
}
