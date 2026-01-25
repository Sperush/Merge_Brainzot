using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BackgroundMusic : MonoBehaviour
{
    public static BackgroundMusic Instance;
    [Header("Audio")]
    public AudioSource audioMusic;
    public AudioClip[] musicClip;
    public Sprite[] MusicSprite;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        } else
        {
            Instance = this;
            LoadSettings();
            DontDestroyOnLoad(gameObject);
        }
    }
    public void ChangeMusic(int typeScene)
    {
        audioMusic.clip = musicClip[typeScene];
        if (!audioMusic.isPlaying)
        {
            audioMusic.Play();
        }
    }
    public void MuteMusic()
    {
        audioMusic.mute = !audioMusic.mute;
        Char.Instance.imgMusic.sprite = MusicSprite[audioMusic.mute ? 1 : 0];
        SaveSettings();
    }
    private void SaveSettings()
    {
        PlayerPrefs.SetInt("SFXMuted", audioMusic.mute ? 1 : 0);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Load âm lượng + mute
    /// </summary>
    private void LoadSettings()
    {
        audioMusic.mute = PlayerPrefs.GetInt("SFXMuted", 0) == 1;
    }
}
