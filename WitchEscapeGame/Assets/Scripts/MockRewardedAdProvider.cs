using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// A fake ad provider for testing the full revive flow in the Editor
/// with NO real SDK installed.
///
/// Behaviour:
/// - Always reports IsAdReady = true after a tiny simulated load.
/// - When ShowAd is called, waits 'simulatedWatchSeconds' then grants the reward.
/// - Toggle 'simulateUserSkips' in the Inspector to test the "closed without
///   reward" path (where the game should fall through to normal Game Over).
///
/// This is a MonoBehaviour so you can drop it on a GameObject and tweak it in
/// the Inspector. Real providers can be plain C# classes or MonoBehaviours too.
/// </summary>
public class MockRewardedAdProvider : MonoBehaviour, IRewardedAdProvider
{
    [Header("Mock Settings")]
    [Tooltip("Seconds to simulate the user 'watching' the ad before reward is granted.")]
    [SerializeField] private float simulatedWatchSeconds = 1.5f;

    [Tooltip("If true, ShowAd will simulate the user skipping/closing early (no reward).")]
    [SerializeField] private bool simulateUserSkips = false;

    [Tooltip("Seconds to simulate ad loading before IsAdReady becomes true.")]
    [SerializeField] private float simulatedLoadSeconds = 0.2f;

    private bool _isAdReady;

    public bool IsAdReady => _isAdReady;

    public void LoadAd()
    {
        // Simulate a network load. Real SDKs would call their own Load() here.
        StopAllCoroutines();
        _isAdReady = false;
        StartCoroutine(SimulateLoad());
    }

    private IEnumerator SimulateLoad()
    {
        yield return new WaitForSecondsRealtime(simulatedLoadSeconds);
        _isAdReady = true;
        Debug.Log("[MockAd] Ad loaded and ready.");
    }

    public void ShowAd(Action onRewardGranted, Action onAdClosedWithoutReward)
    {
        if (!_isAdReady)
        {
            Debug.LogWarning("[MockAd] ShowAd called but no ad ready. Treating as no-reward.");
            onAdClosedWithoutReward?.Invoke();
            return;
        }

        StartCoroutine(SimulateWatch(onRewardGranted, onAdClosedWithoutReward));
    }

    private IEnumerator SimulateWatch(Action onRewardGranted, Action onAdClosedWithoutReward)
    {
        Debug.Log("[MockAd] Ad playing... (simulated)");

        // IMPORTANT: use realtime wait because Time.timeScale is 0 on death.
        yield return new WaitForSecondsRealtime(simulatedWatchSeconds);

        // Consume this ad; a new one must be loaded for next time.
        _isAdReady = false;

        if (simulateUserSkips)
        {
            Debug.Log("[MockAd] User skipped — NO reward granted.");
            onAdClosedWithoutReward?.Invoke();
        }
        else
        {
            Debug.Log("[MockAd] Ad completed — reward granted.");
            onRewardGranted?.Invoke();
        }
    }
}
