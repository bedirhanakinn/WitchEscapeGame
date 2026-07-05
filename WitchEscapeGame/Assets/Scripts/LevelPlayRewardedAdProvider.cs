using System;
using System.Collections;
using UnityEngine;
using Unity.Services.LevelPlay; // LevelPlay (Ads Mediation) 8.6.0+ namespace.
                                 // NOTE: verify this namespace against your installed
                                 // package — older docs use `using com.unity3d.mediation;`.

/// <summary>
/// Real ad provider backed by Unity LevelPlay (formerly Unity Ads + ironSource).
///
/// It implements YOUR existing IRewardedAdProvider contract, so nothing in
/// ReviveController / ReviveButtonUI / GameManager has to change. To go live,
/// assign THIS component wherever you currently assign MockRewardedAdProvider.
///
/// IMPORTANT ASYNC CAVEAT (from LevelPlay docs):
/// OnAdRewarded and OnAdClosed are asynchronous and can arrive in EITHER order.
/// The reward event can fire AFTER close. This adapter treats "reward earned"
/// as the source of truth and uses a short grace window after close before
/// deciding "no reward", so a late reward is never lost and the player's revive
/// isn't wrongly denied.
/// </summary>
public class LevelPlayRewardedAdProvider : MonoBehaviour, IRewardedAdProvider
{
    [Header("LevelPlay Keys (from the dashboard)")]
    [Tooltip("Your Android App Key ('Witch Hunt!' entry) from the LevelPlay Apps page.")]
    [SerializeField] private string androidAppKey = "2707cc80d";
    [Tooltip("Your iOS App Key ('Witch Hunt' entry) from the LevelPlay Apps page.")]
    [SerializeField] private string iosAppKey = "2707d0195";
    [Tooltip("Rewarded Ad Unit ID for Android (your 'Revive ad android' unit).")]
    [SerializeField] private string androidRewardedAdUnitId = "gsqr6q9t3bcqs5c2";
    [Tooltip("Rewarded Ad Unit ID for iOS (your 'Revive ad ios' unit).")]
    [SerializeField] private string iosRewardedAdUnitId = "x252ykyp53a148h6";

    [Header("Behaviour")]
    [Tooltip("Optional placement name set up in the dashboard. Leave empty to use the ad unit default.")]
    [SerializeField] private string placementName = "";
    [Tooltip("Seconds to wait after the ad closes for a late OnAdRewarded before deciding 'no reward'.")]
    [SerializeField] private float rewardGraceSeconds = 0.75f;
    [Tooltip("Seconds to wait before retrying a failed load.")]
    [SerializeField] private float loadRetrySeconds = 4f;

    private LevelPlayRewardedAd _rewardedAd;
    private bool _initialized;

    // Per-show state
    private Action _onRewardGranted;
    private Action _onClosedWithoutReward;
    private bool _earnedRewardThisShow;
    private bool _outcomeResolved;
    private Coroutine _graceRoutine;

    // IRewardedAdProvider ---------------------------------------------------

    public bool IsAdReady =>
        _initialized && _rewardedAd != null && _rewardedAd.IsAdReady();

    public void LoadAd()
    {
        if (!_initialized || _rewardedAd == null)
        {
            // Not ready to load yet; init flow will trigger the first load itself.
            return;
        }
        _rewardedAd.LoadAd();
    }

    public void ShowAd(Action onRewardGranted, Action onAdClosedWithoutReward)
    {
        // Store this show's callbacks and reset outcome tracking.
        _onRewardGranted = onRewardGranted;
        _onClosedWithoutReward = onAdClosedWithoutReward;
        _earnedRewardThisShow = false;
        _outcomeResolved = false;
        if (_graceRoutine != null) { StopCoroutine(_graceRoutine); _graceRoutine = null; }

        if (!IsAdReady)
        {
            // No ad to show — fall through to normal Game Over, exactly like the mock's guard.
            Debug.LogWarning("[LevelPlay] ShowAd called but no ad ready. Treating as no-reward.");
            ResolveOutcome();
            return;
        }

        // Empty placement -> use the no-arg overload (as in the sample) so the SDK
        // serves the ad unit's default reward. Otherwise pass the named placement.
        if (string.IsNullOrEmpty(placementName)) _rewardedAd.ShowAd();
        else _rewardedAd.ShowAd(placementName);
    }

    // Lifecycle -------------------------------------------------------------

    private void Start()
    {
        InitLevelPlay();
    }

    private void InitLevelPlay()
    {
#if UNITY_ANDROID
        string appKey = androidAppKey;
#elif UNITY_IOS
        string appKey = iosAppKey;
#else
        string appKey = androidAppKey; // Editor fallback for testing the flow.
#endif

        if (string.IsNullOrEmpty(appKey))
        {
            Debug.LogError("[LevelPlay] App Key is empty. Fill it in the Inspector.");
            return;
        }

        // Confirmed against the installed LevelPlay sample: init takes just the app key.
        // (There is also an Init(appKey, userId) overload if you need server-to-server
        // rewarded callbacks later.)
        LevelPlay.OnInitSuccess += OnInitSuccess;
        LevelPlay.OnInitFailed += OnInitFailed;
        LevelPlay.Init(appKey);
    }

    private void OnInitSuccess(LevelPlayConfiguration config)
    {
        _initialized = true;

        // The rewarded ad object MUST be created after init success.
#if UNITY_ANDROID
        string adUnitId = androidRewardedAdUnitId;
#elif UNITY_IOS
        string adUnitId = iosRewardedAdUnitId;
#else
        string adUnitId = androidRewardedAdUnitId;
#endif

        _rewardedAd = new LevelPlayRewardedAd(adUnitId);
        _rewardedAd.OnAdLoaded      += HandleAdLoaded;
        _rewardedAd.OnAdLoadFailed  += HandleAdLoadFailed;
        _rewardedAd.OnAdDisplayed   += HandleAdDisplayed;
        _rewardedAd.OnAdDisplayFailed += HandleAdDisplayFailed;
        _rewardedAd.OnAdRewarded    += HandleAdRewarded;
        _rewardedAd.OnAdClicked     += HandleAdClicked;
        _rewardedAd.OnAdClosed      += HandleAdClosed;

        _rewardedAd.LoadAd(); // Pre-cache so there's no wait at the death screen.
    }

    private void OnInitFailed(LevelPlayInitError error)
    {
        Debug.LogError($"[LevelPlay] Init failed: {error}");
    }

    // Ad event handlers -----------------------------------------------------
    // (LevelPlay callbacks run on the main thread, so coroutines are safe here.)

    private void HandleAdLoaded(LevelPlayAdInfo info)
    {
        Debug.Log("[LevelPlay] Rewarded ad loaded.");
    }

    private void HandleAdLoadFailed(LevelPlayAdError error)
    {
        Debug.LogWarning($"[LevelPlay] Load failed: {error}. Retrying in {loadRetrySeconds}s.");
        StartCoroutine(RetryLoad());
    }

    private IEnumerator RetryLoad()
    {
        yield return new WaitForSecondsRealtime(loadRetrySeconds);
        LoadAd();
    }

    private void HandleAdDisplayed(LevelPlayAdInfo info) { }

    private void HandleAdDisplayFailed(LevelPlayAdInfo info, LevelPlayAdError error)
    {
        Debug.LogWarning($"[LevelPlay] Display failed: {error}. Falling through to no-reward.");
        ResolveOutcome();       // no reward
        LoadAd();               // try to have one ready for next time
    }

    private void HandleAdRewarded(LevelPlayAdInfo info, LevelPlayReward reward)
    {
        // Reward is the source of truth. It may arrive before OR after close.
        _earnedRewardThisShow = true;

        // If close already happened (grace running), resolve immediately as granted.
        if (_graceRoutine != null)
        {
            StopCoroutine(_graceRoutine);
            _graceRoutine = null;
            ResolveOutcome();
        }
    }

    private void HandleAdClicked(LevelPlayAdInfo info) { }

    private void HandleAdClosed(LevelPlayAdInfo info)
    {
        // Wait a beat in case OnAdRewarded is late, then resolve.
        if (_outcomeResolved) return;
        _graceRoutine = StartCoroutine(GraceThenResolve());
    }

    private IEnumerator GraceThenResolve()
    {
        yield return new WaitForSecondsRealtime(rewardGraceSeconds);
        _graceRoutine = null;
        ResolveOutcome();
    }

    // Outcome resolution (exactly once per show) ----------------------------

    private void ResolveOutcome()
    {
        if (_outcomeResolved) return;
        _outcomeResolved = true;

        var granted = _onRewardGranted;
        var closed = _onClosedWithoutReward;
        _onRewardGranted = null;
        _onClosedWithoutReward = null;

        if (_earnedRewardThisShow) granted?.Invoke();
        else closed?.Invoke();

        LoadAd(); // Pre-cache the next ad after every show.
    }

    private void OnDestroy()
    {
        LevelPlay.OnInitSuccess -= OnInitSuccess;
        LevelPlay.OnInitFailed -= OnInitFailed;

        if (_rewardedAd != null)
        {
            _rewardedAd.OnAdLoaded      -= HandleAdLoaded;
            _rewardedAd.OnAdLoadFailed  -= HandleAdLoadFailed;
            _rewardedAd.OnAdDisplayed   -= HandleAdDisplayed;
            _rewardedAd.OnAdDisplayFailed -= HandleAdDisplayFailed;
            _rewardedAd.OnAdRewarded    -= HandleAdRewarded;
            _rewardedAd.OnAdClicked     -= HandleAdClicked;
            _rewardedAd.OnAdClosed      -= HandleAdClosed;
        }
    }
}
