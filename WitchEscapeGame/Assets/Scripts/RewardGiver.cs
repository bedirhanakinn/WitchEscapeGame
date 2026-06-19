using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Universal reward dispenser. Add to any GameObject and call GiveReward()
/// (from code, a UnityEvent, a Button onClick, a trigger, an animation event,
/// whatever) to grant the player extra score points and/or coins.
///
/// Hooks into your existing ScoreManager.AddBonus and CurrencyManager.AddCoins.
/// Adjust the amounts in the Inspector — no code changes needed per use.
/// </summary>
public class RewardGiver : MonoBehaviour
{
    [Header("Reward Amounts")]
    [Tooltip("Score points to award. Set to 0 to give no points.")]
    [SerializeField] private int points = 100;

    [Tooltip("Coins/currency to award. Set to 0 to give no coins.")]
    [SerializeField] private int coins = 10;

    [Header("Score Label")]
    [Tooltip("Label shown in the floating text when points are awarded.")]
    [SerializeField] private string pointsLabel = "Bonus!";

    [Header("Options")]
    [Tooltip("If true, this reward can only be given once (e.g. a pickup). " +
             "If false, it can be triggered repeatedly.")]
    [SerializeField] private bool onceOnly = false;

    [Tooltip("Optional: trigger this on collision/trigger enter with the player.")]
    [SerializeField] private bool giveOnPlayerTrigger = false;

    [Tooltip("Optional: trigger the reward automatically when this GameObject " +
             "becomes active (SetActive(true) or OnEnable). Useful when a model " +
             "is activated on collision and that activation should grant points.")]
    [SerializeField] private bool giveOnEnable = false;

    [SerializeField] private string playerTag = "Player";

    [Header("Hooks (optional)")]
    [Tooltip("Fired after the reward is granted — wire SFX, particles, etc.")]
    [SerializeField] private UnityEvent onRewardGiven;

    private bool _given;

    /// <summary>
    /// Grants the configured points and coins. Call this from anywhere.
    /// </summary>
    public void GiveReward()
    {
        if (onceOnly && _given) return;
        _given = true;

        // Award score points via the existing ScoreManager bonus system.
        if (points != 0 && ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddBonus(points, pointsLabel);
        }

        // Award coins via the existing CurrencyManager.
        if (coins != 0 && CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.AddCoins(coins);
        }

        onRewardGiven?.Invoke();
    }

    /// <summary>
    /// Convenience overloads if you want to override amounts at call time
    /// without changing the Inspector values.
    /// </summary>
    public void GiveReward(int customPoints, int customCoins)
    {
        if (onceOnly && _given) return;
        _given = true;

        if (customPoints != 0 && ScoreManager.Instance != null)
            ScoreManager.Instance.AddBonus(customPoints, pointsLabel);

        if (customCoins != 0 && CurrencyManager.Instance != null)
            CurrencyManager.Instance.AddCoins(customCoins);

        onRewardGiven?.Invoke();
    }

    void OnEnable()
    {
        if (giveOnEnable)
            GiveReward();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!giveOnPlayerTrigger) return;
        if (!other.CompareTag(playerTag)) return;
        GiveReward();
    }
}