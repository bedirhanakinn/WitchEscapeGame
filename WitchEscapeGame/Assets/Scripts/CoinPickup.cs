using UnityEngine;

/// <summary>
/// Attach to a coin GameObject with a 2D Trigger collider.
/// On collision with the player, credits both permanent currency AND the
/// per-run tracker, then destroys itself.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class CoinPickup : MonoBehaviour
{
    [Tooltip("Coins awarded for this pickup. 1 for regular coins, more for special variants.")]
    [SerializeField] private int coinValue = 1;

    [Tooltip("Tag the player GameObject must have. Default is 'Player'.")]
    [SerializeField] private string playerTag = "Player";

    [Header("Feedback (optional)")]
    [SerializeField] private GameObject pickupVfxPrefab;

    private bool collected; // prevents double-credit if multiple colliders touch in same frame

    void Reset()
    {
        // Auto-set collider to trigger when added in editor
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (collected) return;
        if (!other.CompareTag(playerTag)) return;

        collected = true;

        // Bank permanently (saves to PlayerPrefs immediately)
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.AddCoins(coinValue);

        // Track for this-run HUD/summary
        if (RunCoinTracker.Instance != null)
            RunCoinTracker.Instance.AddRunCoin(coinValue);

        // Optional feedback
        if (pickupVfxPrefab != null)
            Instantiate(pickupVfxPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }
}
