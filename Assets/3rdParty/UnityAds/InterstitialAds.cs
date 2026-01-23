//using UnityEngine;
//using Unity.Services.LevelPlay;
//using System.Collections;


//public class InterstitialAds : MonoBehaviour
//{
//    public static InterstitialAds Instance { get; private set; }
//    [SerializeField] private string interstitialAdUnitId;
//    private LevelPlayInterstitialAd interstitialAd;
//    private System.Action onAdCompleteCallback;

//    private void Awake()
//    {
//        // If there is an instance, and it's not me, delete myself.
//        if (Instance != null && Instance != this)
//        {
//            Destroy(this);
//        }
//        else
//        {
//            Instance = this;
//        }
//    }

//    private void Start()
//    {
        
//    }

//    public IEnumerator ShowAdsOnStart()
//    {
//        yield return new WaitForSeconds(1.5f);
//        while(!interstitialAd.IsAdReady())
//        {
//            yield return new WaitForSeconds(1f);
//        }    
//        ShowInterstitial();
//        yield return null;
//    }    

//    public void EnableAds()
//    {
//        interstitialAd = new LevelPlayInterstitialAd(interstitialAdUnitId);

//        interstitialAd.OnAdLoaded += InterstitialOnAdLoadedEvent;
//        interstitialAd.OnAdLoadFailed += InterstitialOnAdLoadFailedEvent;
//        interstitialAd.OnAdDisplayed += InterstitialOnAdDisplayedEvent;
//        interstitialAd.OnAdDisplayFailed += InterstitialOnAdDisplayFailedEvent;
//        interstitialAd.OnAdClicked += InterstitialOnAdClickedEvent;
//        interstitialAd.OnAdClosed += InterstitialOnAdClosedEvent;
//        interstitialAd.OnAdInfoChanged += InterstitialOnAdInfoChangedEvent;
        
//        // Load interstitial ad as soon as it is enabled
//        LoadInterstitial();
//    }   

//    public void LoadInterstitial()
//    {
//        // Ad load
//        interstitialAd.LoadAd();
//    }

//    public void ShowInterstitial(System.Action onAdComplete = null)
//    {
//        onAdCompleteCallback = onAdComplete;
//        if (interstitialAd.IsAdReady())
//        {
//            interstitialAd.ShowAd();
//        }
//        else
//        {
//            Debug.Log("unity-script: Levelplay Interstital Ad Ready? - False");
//        }
//    }

//    void InterstitialOnAdLoadedEvent(LevelPlayAdInfo adInfo)
//    {
//        Debug.Log("unity-script: I got InterstitialOnAdLoadedEvent With AdInfo " + adInfo);
//    }

//    void InterstitialOnAdLoadFailedEvent(LevelPlayAdError error)
//    {
//        Debug.Log("unity-script: I got InterstitialOnAdLoadFailedEvent With Error " + error);
//        onAdCompleteCallback?.Invoke(); // Execute the callback on ad completion
//    }

//    void InterstitialOnAdDisplayedEvent(LevelPlayAdInfo adInfo)
//    {
//        Debug.Log("unity-script: I got InterstitialOnAdDisplayedEvent With AdInfo " + adInfo);
        
//    }

//    private void InterstitialOnAdDisplayFailedEvent(LevelPlayAdInfo adInfo, LevelPlayAdError error)
//    {
//        Debug.LogWarning($"InterstitialOnAdDisplayFailedEvent adInfo={adInfo}, error={error}");
//        onAdCompleteCallback?.Invoke();
//        LoadInterstitial();
//    }

//    void InterstitialOnAdClickedEvent(LevelPlayAdInfo adInfo)
//    {
//        Debug.Log("unity-script: I got InterstitialOnAdClickedEvent With AdInfo " + adInfo);
//    }

//    void InterstitialOnAdClosedEvent(LevelPlayAdInfo adInfo)
//    {
//        Debug.Log("unity-script: I got InterstitialOnAdClosedEvent With AdInfo " + adInfo);
//        onAdCompleteCallback?.Invoke(); // Execute the callback on ad completion
//        LoadInterstitial(); // Load the next ad
//    }

//    void InterstitialOnAdInfoChangedEvent(LevelPlayAdInfo adInfo)
//    {
//        Debug.Log("unity-script: I got InterstitialOnAdInfoChangedEvent With AdInfo " + adInfo);
//    }

//    private void OnDisable()
//    {
//        interstitialAd?.DestroyAd();
//    }

//    public void ShowInterstitialFromButton()
//    {
//        ShowInterstitial();
//    }


//    public void LoadInterstitialFromButton()
//    {
//        LoadInterstitial();
//    }

//}