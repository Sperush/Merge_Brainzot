//using UnityEngine;
//using System;
//using Unity.Services.LevelPlay;

//public class RewardedAds : MonoBehaviour
//{
//    public static RewardedAds Instance { get; private set; }

//    [SerializeField] private string rewardedAdUnitId;

//    private LevelPlayRewardedAd rewardedAd;
//    private bool isLoading;
//    private bool rewardGrantedThisShow;
//    private Action<bool> onAdCompleteCallback; // true = được reward, false = fail/skip

//    private void Awake()
//    {
//        if (Instance != null && Instance != this)
//        {
//            Destroy(gameObject);
//            return;
//        }

//        Instance = this;
//        DontDestroyOnLoad(gameObject);
//        EnableAds();
//    }

//    public void EnableAds()
//    {
//        if (rewardedAd != null)
//            return;

//        rewardedAd = new LevelPlayRewardedAd(rewardedAdUnitId);

//        rewardedAd.OnAdLoaded += RewardedOnAdLoadedEvent;
//        rewardedAd.OnAdLoadFailed += RewardedOnAdLoadFailedEvent;
//        rewardedAd.OnAdDisplayed += RewardedOnAdDisplayedEvent;
//        rewardedAd.OnAdDisplayFailed += RewardedOnAdDisplayFailedEvent;
//        rewardedAd.OnAdClosed += RewardedOnAdClosedEvent;
//        rewardedAd.OnAdRewarded += RewardedOnAdRewardedEvent;
//        rewardedAd.OnAdClicked += RewardedOnAdClickedEvent;

//        LoadRewardedInternal();
//    }

//    /// <summary>
//    /// Hàm cũ LoadRewardedAd – hiện tại dùng để SHOW ad.
//    /// onComplete(true)  = user được reward
//    /// onComplete(false) = không xem hết / fail / không có ad
//    /// </summary>
//    public void LoadRewardedAd(Action<bool> onComplete = null)
//    {
//        onAdCompleteCallback = onComplete;

//        if (rewardedAd == null)
//        {
//            Debug.LogWarning("RewardedAds: rewardedAd is null, call EnableAds() first.");
//            //  GameManager.Instance.Respawn(false);
//            onAdCompleteCallback?.Invoke(false);
//            return;
//        }

//        if (rewardedAd.IsAdReady())
//        {
//            rewardGrantedThisShow = false;
//            rewardedAd.ShowAd();
//        }
//        else
//        {
//            Debug.Log("unity-script: RewardedAd IsAdReady - False");
//            // GameManager.Instance.Respawn(false);
//            onAdCompleteCallback?.Invoke(false);
//            LoadRewardedInternal(); // chuẩn bị cho lần sau
//        }
//    }

//    private void LoadRewardedInternal()
//    {
//        if (rewardedAd == null || isLoading)
//            return;

//        isLoading = true;
//        Debug.Log("unity-script: RewardedAd Loading...");
//        rewardedAd.LoadAd();
//    }

//    // ==== EVENTS ====

//    private void RewardedOnAdLoadedEvent(LevelPlayAdInfo adInfo)
//    {
//        isLoading = false;
//        Debug.Log("unity-script: RewardedOnAdLoadedEvent " + adInfo);
//    }

//    private void RewardedOnAdLoadFailedEvent(LevelPlayAdError error)
//    {
//        isLoading = false;
//        Debug.Log("unity-script: RewardedOnAdLoadFailedEvent " + error);
//        // Không auto Respawn ở đây – chỉ khi người chơi thực sự bấm xem mà fail
//    }

//    private void RewardedOnAdDisplayedEvent(LevelPlayAdInfo adInfo)
//    {
//        Debug.Log("unity-script: RewardedOnAdDisplayedEvent " + adInfo);
//    }

//    private void RewardedOnAdDisplayFailedEvent(LevelPlayAdInfo adInfo, LevelPlayAdError error)
//    {
//        Debug.LogWarning($"RewardedOnAdDisplayFailedEvent adInfo={adInfo}, error={error}");
//        // GameManager.Instance.Respawn(false);
//        onAdCompleteCallback?.Invoke(false);
//        LoadRewardedInternal();
//    }

//    private void RewardedOnAdClosedEvent(LevelPlayAdInfo adInfo)
//    {
//        Debug.Log("unity-script: RewardedOnAdClosedEvent " + adInfo);

//        // Nếu không được reward trong lần show này → coi như fail/skip
//        if (!rewardGrantedThisShow)
//        {
//            //  GameManager.Instance.Respawn(false);
//            onAdCompleteCallback?.Invoke(false);
//        }

//        LoadRewardedInternal();
//    }

//    private void RewardedOnAdRewardedEvent(LevelPlayAdInfo adInfo, LevelPlayReward reward)
//    {
//        rewardGrantedThisShow = true;
//        Debug.Log($"unity-script: RewardedOnAdRewardedEvent Reward: {reward.Name} x {reward.Amount}");

//        // Đây là chỗ bạn thực sự "Grant a reward" (ví dụ Item, Coin,...)
//        // Nếu muốn giữ logic cũ, chỉ gọi callback:
//        onAdCompleteCallback?.Invoke(true);
//    }

//    private void RewardedOnAdClickedEvent(LevelPlayAdInfo adInfo)
//    {
//        Debug.Log("unity-script: RewardedOnAdClickedEvent " + adInfo);
//    }

//    private void OnDisable()
//    {
//        if (rewardedAd != null)
//        {
//            rewardedAd.DestroyAd();
//            rewardedAd = null;
//        }
//    }
//}
