using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CloseShaveTrigger : MonoBehaviour
{
    [Tooltip("Override the default ScoreManager.closeShaveBonus. Leave 0 to use the manager default.")]
    [SerializeField] private int customBonus = 0;

    [Tooltip("Label shown in floating text.")]
    [SerializeField] private string label = "Close Shave!";

    [SerializeField] private string playerTag = "Player";

    [Header("Audio")]
    [SerializeField] private AudioClip closeShaveSound;
    [SerializeField] private float soundVolume = 1f;
    [SerializeField] private float minPitch = 0.95f;
    [SerializeField] private float maxPitch = 1.10f;

    private bool fired;

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col != null)
            col.isTrigger = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (fired) return;
        if (!other.CompareTag(playerTag)) return;
        if (GameManager.instance.IsGameOver) return;
        if (Time.timeScale <= 0) return;

        fired = true;

        // Play Close Shave sound
        if (closeShaveSound != null)
        {
            float pitch = Random.Range(minPitch, maxPitch);
            SoundManager.Instance.PlaySFX(closeShaveSound, soundVolume, pitch);
        }

        int bonus = customBonus > 0 ? customBonus : 500;
        ScoreManager.Instance.AddBonus(bonus, label);
    }
}