using UnityEngine;

public class CollectiblePickup : MonoBehaviour
{
    public CollectibleType collectibleType;

    [Header("Audio")]
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private float soundVolume = 1f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PowerUpManager manager = collision.GetComponent<PowerUpManager>();

        if (manager != null)
        {
            // Play pickup sound
            if (pickupSound != null)
            {
                SoundManager.Instance.PlaySFX(pickupSound, soundVolume);
            }

            manager.Collect(collectibleType);

            Destroy(gameObject);
        }
    }
}