using UnityEngine;

/// <summary>
/// Attach to a coin GameObject with a 2D Trigger collider.
/// On collision with the player (or any power-up state), credits both permanent
/// currency AND the per-run tracker, then destroys itself.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class CoinPickup : MonoBehaviour
{
    [Tooltip("Coins awarded for this pickup.")]
    [SerializeField] private int coinValue = 1;

    [Tooltip("Tag on the Player root GameObject.")]
    [SerializeField] private string playerTag = "Player";

    [Header("Audio")]
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private float soundVolume = 1f;
    [SerializeField] private float minPitch = 0.95f;
    [SerializeField] private float maxPitch = 1.15f;

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
        if (collected) return;
        if (!IsPlayer(other)) return;

        collected = true;

        // Play coin sound
        if (pickupSound != null)
        {
            float pitch = Random.Range(minPitch, maxPitch);
            SoundManager.Instance.PlaySFX(pickupSound, soundVolume, pitch);
        }

        // Add permanent coins
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.AddCoins(coinValue);

        // Add run coins
        if (RunCoinTracker.Instance != null)
            RunCoinTracker.Instance.AddRunCoin(coinValue);

        // Spawn VFX
        if (pickupVfxPrefab != null)
            Instantiate(pickupVfxPrefab, transform.position, Quaternion.identity);

        Destroy(gameObject);
    }

    /// <summary>
    /// Returns true if the collider belongs to the player or any of their
    /// power-up child GameObjects.
    /// </summary>
    private bool IsPlayer(Collider2D other)
    {
        if (other.CompareTag(playerTag))
            return true;

        Transform t = other.transform.parent;

        while (t != null)
        {
            if (t.CompareTag(playerTag))
                return true;

            t = t.parent;
        }

        return false;
    }
}