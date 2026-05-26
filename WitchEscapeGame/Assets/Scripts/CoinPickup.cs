using UnityEngine;

/// <summary>
/// Attach to a coin GameObject with a 2D Trigger collider.
/// On collision with the player (or any power-up state), credits both permanent
/// currency AND the per-run tracker, then destroys itself.
///
/// Detection works in two ways:
/// 1. Direct tag match: the colliding GameObject has playerTag ("Player")
/// 2. Parent tag match: the colliding GameObject's parent (or any ancestor)
///    has playerTag — catches power-up states (LovePower, FrogPower, etc.)
///    that are children of the Player root but have their own tags.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class CoinPickup : MonoBehaviour
{
    [Tooltip("Coins awarded for this pickup.")]
    [SerializeField] private int coinValue = 1;

    [Tooltip("Tag on the Player root GameObject.")]
    [SerializeField] private string playerTag = "Player";

    [Header("Feedback (optional)")]
    [SerializeField] private GameObject pickupVfxPrefab;

    private bool collected;

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        ///Debug.Log($"Coin touched by: {other.gameObject.name}, tag: {other.tag}, parent: {other.transform.parent?.name}");
        if (collected) return;
        if (!IsPlayer(other)) return;

        collected = true;

        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.AddCoins(coinValue);

        if (RunCoinTracker.Instance != null)
            RunCoinTracker.Instance.AddRunCoin(coinValue);

        if (pickupVfxPrefab != null)
            Instantiate(pickupVfxPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }

    /// <summary>
    /// Returns true if the collider belongs to the player or any of their
    /// power-up child GameObjects. Walks up the hierarchy looking for playerTag.
    /// </summary>
    private bool IsPlayer(Collider2D other)
    {
        // Direct match (normal gameplay)
        if (other.CompareTag(playerTag)) return true;

        // Walk up the parent chain (power-up states are children of Player root)
        Transform t = other.transform.parent;
        while (t != null)
        {
            if (t.CompareTag(playerTag)) return true;
            t = t.parent;
        }

        return false;
    }
}