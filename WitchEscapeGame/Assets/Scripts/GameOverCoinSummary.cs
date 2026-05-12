using System;
using TMPro;
using UnityEngine;

/// <summary>
/// Shows total coins collected this run on the GameOver panel, with a hype
/// message that scales with the coin count. Tiers and messages are editable
/// in the Inspector — no code changes needed to rewrite or rebalance them.
///
/// Attach to the GameOver UIMenu. Wire its onShow event to RefreshDisplay().
/// </summary>
public class GameOverCoinSummary : MonoBehaviour
{
    [Serializable]
    public class HypeTier
    {
        [Tooltip("Inclusive lower bound. A count of exactly this number qualifies for this tier.")]
        public int minCoins;

        [Tooltip("Random message picked from this pool. Add multiple for variety.")]
        public string[] messages;
    }

    [Header("Text targets")]
    [SerializeField] private TextMeshProUGUI countText;
    [SerializeField] private TextMeshProUGUI hypeText;

    [Header("Format")]
    [Tooltip("Use {0} where the coin count should appear.")]
    [SerializeField] private string countFormat = "Total coins collected: {0}";

    [Header("Hype tiers (order doesn't matter, sorted at runtime)")]
    [Tooltip("Highest minCoins tier the count qualifies for is selected.")]
    [SerializeField] private HypeTier[] tiers;

    /// <summary>
    /// Wire this to GameOver UIMenu's onShow UnityEvent in the Inspector.
    /// </summary>
    public void RefreshDisplay()
    {
        int coins = RunCoinTracker.Instance != null ? RunCoinTracker.Instance.CoinsThisRun : 0;

        if (countText != null)
            countText.text = string.Format(countFormat, coins);

        if (hypeText != null)
            hypeText.text = PickMessage(coins);
    }

    private string PickMessage(int coins)
    {
        if (tiers == null || tiers.Length == 0) return "";

        // Find the highest-minCoins tier the count qualifies for
        HypeTier best = null;
        for (int i = 0; i < tiers.Length; i++)
        {
            var t = tiers[i];
            if (t == null) continue;
            if (coins < t.minCoins) continue;
            if (best == null || t.minCoins > best.minCoins) best = t;
        }

        if (best == null || best.messages == null || best.messages.Length == 0) return "";
        return best.messages[UnityEngine.Random.Range(0, best.messages.Length)];
    }
}
