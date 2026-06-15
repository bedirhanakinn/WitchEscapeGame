using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Coordinates the rewarded-ad revive flow and owns the "once per run" state.
///
/// RESPONSIBILITIES (gameplay/state only — NOT UI):
/// - Track whether the player has already revived this run.
/// - Ask the IRewardedAdProvider to show an ad when the player requests a revive.
/// - On reward granted: run the 3-2-1 countdown, then restore control.
/// - On no reward: fire an event so the UI falls through to normal Game Over.
///
/// This script does NOT know about buttons, sprites, or text. The UI layer
/// subscribes to its events and calls RequestRevive(). This keeps UI and
/// gameplay-state decoupled and makes the system reusable across games.
///
/// Designed to sit alongside your existing GameManager. It reads GameManager
/// for game-over state and calls back into it to resume the run.
/// </summary>
public class ReviveController : MonoBehaviour
{
    public static ReviveController Instance { get; private set; }

    [Header("Ad Provider")]
    [Tooltip("Drag any component implementing IRewardedAdProvider here " +
             "(MockRewardedAdProvider for testing, or a real SDK wrapper). " +
             "Must be a MonoBehaviour on a GameObject.")]
    [SerializeField] private MonoBehaviour adProviderBehaviour;

    [Header("Countdown")]
    [Tooltip("Number to count down from before restoring control (3 = '3,2,1').")]
    [SerializeField] private int countdownFrom = 3;
    [Tooltip("Real seconds each countdown number is shown.")]
    [SerializeField] private float countdownStepSeconds = 1f;

    // ---- Events the UI / gameplay layers subscribe to ----

    /// <summary>Fired when the ad grants its reward (before countdown starts).</summary>
    public event Action OnAdRewardGranted;

    /// <summary>Fired each countdown tick with the current number (3, then 2, then 1).</summary>
    public event Action<int> OnReviveCountdownTick;

    /// <summary>Fired when the countdown finishes and control is restored.</summary>
    public event Action OnReviveCountdownComplete;

    /// <summary>
    /// Fired when a revive attempt ends WITHOUT reviving (ad skipped/failed,
    /// or revive already used). UI should proceed to the normal Game Over flow.
    /// </summary>
    public event Action OnReviveUnavailableOrDeclined;

    /// <summary>
    /// Inspector-friendly mirror of OnReviveCountdownComplete, in case you want
    /// to wire effects (sound, particles) directly in the Inspector.
    /// </summary>
    public UnityEvent onReviveComplete;

    // ---- State ----

    private IRewardedAdProvider _adProvider;
    private bool _hasRevivedThisRun;
    private bool _reviveInProgress;

    /// <summary>
    /// True if the revive option should be offered right now:
    /// not yet used this run, an ad is ready, and we're not mid-revive.
    /// UI checks this to decide whether to show/enable the "Watch Ad" button.
    /// </summary>
    public bool CanOfferRevive =>
        !_hasRevivedThisRun &&
        !_reviveInProgress &&
        _adProvider != null &&
        _adProvider.IsAdReady;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Resolve the interface from the assigned MonoBehaviour.
        _adProvider = adProviderBehaviour as IRewardedAdProvider;
        if (_adProvider == null)
        {
            Debug.LogError("[ReviveController] Assigned adProviderBehaviour does NOT " +
                           "implement IRewardedAdProvider. Revive will be unavailable.");
        }
    }

    void Start()
    {
        // Pre-load an ad so it's ready by the time the player might die.
        _adProvider?.LoadAd();
    }

    /// <summary>
    /// Call this at the start of every new run (e.g. from GameStart.onGameStarted)
    /// to reset the once-per-run flag and pre-load a fresh ad.
    /// </summary>
    public void ResetForNewRun()
    {
        _hasRevivedThisRun = false;
        _reviveInProgress = false;
        _adProvider?.LoadAd();
    }

    /// <summary>
    /// Called by the UI when the player taps "Watch Ad to Revive".
    /// </summary>
    public void RequestRevive()
    {
        if (!CanOfferRevive)
        {
            // Either already used, no ad, or mid-revive. Fall through to Game Over.
            OnReviveUnavailableOrDeclined?.Invoke();
            return;
        }

        _reviveInProgress = true;

        _adProvider.ShowAd(
            onRewardGranted: HandleRewardGranted,
            onAdClosedWithoutReward: HandleNoReward
        );
    }

    private void HandleRewardGranted()
    {
        // Lock the revive for the rest of this run.
        _hasRevivedThisRun = true;

        OnAdRewardGranted?.Invoke();

        // Run the countdown on realtime since Time.timeScale is 0 during death.
        StartCoroutine(ReviveCountdownRoutine());
    }

    private void HandleNoReward()
    {
        _reviveInProgress = false;
        // User skipped or ad failed — go to normal Game Over.
        OnReviveUnavailableOrDeclined?.Invoke();
    }

    private IEnumerator ReviveCountdownRoutine()
    {
        // Count down N..1, firing a tick each step so the UI can show the number.
        for (int n = countdownFrom; n >= 1; n--)
        {
            OnReviveCountdownTick?.Invoke(n);
            yield return new WaitForSecondsRealtime(countdownStepSeconds);
        }

        // Restore control. The actual gameplay reset is delegated to whoever
        // subscribes to OnReviveCountdownComplete (e.g. GameManager / PlayerController)
        // so this controller stays gameplay-agnostic.
        _reviveInProgress = false;
        OnReviveCountdownComplete?.Invoke();
        onReviveComplete?.Invoke();
    }
}
