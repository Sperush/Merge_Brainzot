//using UnityEngine;
//using Unity.Services.LevelPlay;

//public class AdsInitializer : MonoBehaviour
//{
//    public static AdsInitializer Instance { get; private set; }

//    [SerializeField] private string androidAppKey;
//    [SerializeField] private string iosAppKey;

//    private string appKey;
//    private bool isInitialized = false;

//    private void Awake()
//    {
//        if (Instance != null && Instance != this)
//        {
//            Destroy(gameObject);
//            return;
//        }

//        Instance = this;
//        DontDestroyOnLoad(gameObject);
//    }

//    private void Start()
//    {
//        // đăng ký event trước rồi mới Init
//        LevelPlay.OnInitSuccess += OnInitializationComplete;
//        LevelPlay.OnInitFailed  += OnInitializationFailed;
//        LevelPlay.OnImpressionDataReady += OnAdsImpression;
//        InitializeAds();
//        StartCoroutine(InterstitialAds.Instance.ShowAdsOnStart());
//    }

//    private void OnDestroy()
//    {
//        LevelPlay.OnImpressionDataReady -= OnAdsImpression;
//    }

//    public void InitializeAds()
//    {
//#if UNITY_ANDROID
//        appKey = androidAppKey;
//#elif UNITY_IOS
//        appKey = iosAppKey;
//#else
//        appKey = "unexpected_platform";
//#endif


//      //  LevelPlay.SetMetaData("is_test_suite", "enable");

//        InitLevelPlay();
//    }

//    public void OnAdsImpression(LevelPlayImpressionData data)
//    {
//        //AnalyticHandle.instance.OnAdsRevenueImpression((double)data.Revenue, data.AdNetwork, data.MediationAdUnitName, data.AdFormat, data.Placement);
//    }    

//    private void InitLevelPlay()
//    {
//        // Chỉ cần truyền appKey, KHÔNG dùng LevelPlayAdFormat nữa
//        LevelPlay.Init(appKey);
//    }

//    private void OnInitializationComplete(LevelPlayConfiguration config)
//    {
//        Debug.Log("LevelPlay init success: " + config);
//        isInitialized = true;

//      //  LevelPlay.LaunchTestSuite();

//        // Bật các hệ thống ads của bạn
//        BannerAds.Instance.EnableAds();
//        InterstitialAds.Instance.EnableAds();
//        RewardedAds.Instance.EnableAds();
//    }

//    private void OnInitializationFailed(LevelPlayInitError error)
//    {
//        Debug.LogError("LevelPlay init failed: " + error);
//        isInitialized = false;
//    }

//    public bool IsInitialized()
//    {
//        return isInitialized;
//    }
//}
