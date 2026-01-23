using TMPro;
using UnityEngine;
using UnityEngine.Localization;

public class Noti : MonoBehaviour
{
    public TMP_Text noticeText;
    public static Noti Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            gameObject.SetActive(false);
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject); // Đã có một bản tồn tại rồi
        }
    }
    public static string Get(string key, params object[] args)
    {
        return new LocalizedString("Language", key).GetLocalizedString(args);
    }
    public void Show(string key, float duration = 1f)
    {
        noticeText.text = Get(key);
        gameObject.SetActive(true);
        StopAllCoroutines(); // Dừng nếu có Coroutine cũ đang chạy
        StartCoroutine(HideAfterSeconds(duration));
    }
    private System.Collections.IEnumerator HideAfterSeconds(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        gameObject.SetActive(false);
    }
}
