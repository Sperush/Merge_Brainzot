using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine.UI;

public enum GameSound
{
    fightSound,
    coinSound,
    meleeAttackSound,
    rangeAttackSound,
    loseSound,
    victorySound,
    clickButtonSound,
    snapSound,
    bombSound,
    freezeSound,
    planeSound
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Clips")]
    public AudioClip fightClip;
    public AudioClip coinClip;
    public AudioClip meleeAttackClip;
    public AudioClip rangeAttackClip;
    public AudioClip victoryClip;
    public AudioClip loseClip;
    public AudioClip clickButtonClip;
    public AudioClip snapClip;
    public AudioClip bombBoosterClip;
    public AudioClip freezeBoosterClip;
    public AudioClip bombPlaneFlyClip;


    [Header("Audio Source Pool")]
    public AudioSource audioSourcePrefab;
    private List<AudioSource> audioSources = new List<AudioSource>();
    private Dictionary<GameSound, AudioClip> soundMap = new Dictionary<GameSound, AudioClip>();
    

    [Header("Global Volume & Mute")]
    [Range(0f, 1f)]
    public float sfxVolume = 1f;
    public bool isMuted = false;

    [Header("Melee Audio Clip")]
    public AudioClip[] meleeAudioClip;

    [Header("Range Audio Clip")]
    public AudioClip[] rangeAudioClip;

    [Header("UI")]
    public Sprite[] sp;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Tạo pool AudioSource
            for (int i = 0; i < 10; i++)
            {
                AudioSource src = Instantiate(audioSourcePrefab, transform);
                audioSources.Add(src);
            }

            // Map enum -> clip
            soundMap[GameSound.fightSound] = fightClip;
            soundMap[GameSound.coinSound] = coinClip;
            soundMap[GameSound.meleeAttackSound] = meleeAttackClip;
            soundMap[GameSound.rangeAttackSound] = rangeAttackClip;
            soundMap[GameSound.victorySound] = victoryClip;
            soundMap[GameSound.loseSound] = loseClip;
            soundMap[GameSound.clickButtonSound] = clickButtonClip;
            soundMap[GameSound.snapSound] = snapClip;
            soundMap[GameSound.freezeSound] = freezeBoosterClip;
            soundMap[GameSound.bombSound] = bombBoosterClip;
            soundMap[GameSound.planeSound] = bombPlaneFlyClip;

            LoadSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }


    public void Play(GameSound sound, float volum = -1f)
    {
        if (isMuted) return;

        if (soundMap.ContainsKey(sound))
        {
            AudioClip clip = soundMap[sound];
            AudioSource src = audioSources.Find(s => !s.isPlaying);
            if (src == null) src = audioSources[0];

            src.volume = volum == -1 ? sfxVolume : volum;
            src.pitch = 1f;
            src.PlayOneShot(clip);
        }
        else
        {
            Debug.LogWarning("AudioManager: Sound not found " + sound);
        }
    }

    public void PlayUnitSound(int level, MonsterType type, float volum = -1f)
    {
        if (isMuted) return;
        if (level <= 0 || level > meleeAudioClip.Length || level > rangeAudioClip.Length) return;
        AudioClip clip;
        Debug.Log(type + " " + level);
        if (type == MonsterType.Melee)
        {
            clip = meleeAudioClip[level - 1];
        }
        else
        {
            clip = rangeAudioClip[level - 1];
        }
        AudioSource src = audioSources.Find(s => !s.isPlaying);
        if (src == null) src = audioSources[0];

        src.volume = volum == -1 ? sfxVolume : volum;
        src.pitch = 1f;
        src.PlayOneShot(clip);
    }

    /// <summary>
    /// Mute toàn bộ SFX
    /// </summary>
    public void MuteAll()
    {
        isMuted = true;
        SaveSettings();
    }

    /// <summary>
    /// Bật lại âm thanh SFX
    /// </summary>
    public void UnmuteAll()
    {
        isMuted = false;
        SaveSettings();
    }

    /// <summary>
    /// Toggle mute
    /// </summary>
    public void ToggleMute()
    {
        isMuted = !isMuted;
        Char.Instance.imgSound.sprite = sp[isMuted ? 1 : 0];
        Play(GameSound.clickButtonSound);
        SaveSettings();
    }

    /// <summary>
    /// Fade âm lượng SFX từ hiện tại về targetVolume trong duration giây
    /// </summary>
    public void FadeVolume(float targetVolume, float duration)
    {
        DOTween.To(() => sfxVolume, v => sfxVolume = v, targetVolume, duration).SetEase(Ease.Linear).OnComplete(SaveSettings);
    }

    /// <summary>
    /// Save âm lượng + mute
    /// </summary>
    private void SaveSettings()
    {
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.SetInt("SFXMuted", isMuted ? 1 : 0);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Load âm lượng + mute
    /// </summary>
    private void LoadSettings()
    {
        sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 1f);
        isMuted = PlayerPrefs.GetInt("SFXMuted", 0) == 1;
    }
}

