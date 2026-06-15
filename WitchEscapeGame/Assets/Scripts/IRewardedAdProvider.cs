using System;

/// <summary>
/// SDK-agnostic contract for showing rewarded video ads.
///
/// The game NEVER talks to AdMob / Unity Ads / AppLovin directly.
/// It only talks to this interface. To swap ad networks later, write a new
/// implementation of this interface and assign it — no gameplay/UI code changes.
///
/// Implementations live in their own files (e.g. MockRewardedAdProvider,
/// AdMobRewardedAdProvider, UnityAdsRewardedAdProvider).
/// </summary>
public interface IRewardedAdProvider
{
    /// <summary>
    /// True if an ad is loaded and ready to show right now.
    /// UI should check this to decide whether to show/enable the revive button.
    /// </summary>
    bool IsAdReady { get; }

    /// <summary>
    /// Begin loading/caching an ad so it's ready when the player needs it.
    /// Call this early (e.g. on run start) so there's no wait at the death screen.
    /// </summary>
    void LoadAd();

    /// <summary>
    /// Show the rewarded ad.
    /// </summary>
    /// <param name="onRewardGranted">
    /// Invoked ONLY if the user watched enough of the ad to earn the reward.
    /// </param>
    /// <param name="onAdClosedWithoutReward">
    /// Invoked if the user skipped/closed early, or the ad failed to show.
    /// The game should fall through to the normal Game Over flow in this case.
    /// </param>
    void ShowAd(Action onRewardGranted, Action onAdClosedWithoutReward);
}
