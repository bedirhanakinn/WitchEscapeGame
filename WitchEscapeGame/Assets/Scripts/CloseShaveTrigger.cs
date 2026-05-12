using UnityEngine;

/// <summary>
/// Attach to an empty GameObject placed near an obstacle (above/below/around).
/// Must have a 2D Trigger collider.
/// When the player passes through, awards a bonus and triggers floating text.
/// Single-fire: each trigger only awards once per platform spawn.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class CloseShaveTrigger : MonoBehaviour
{
    [Tooltip("Override the default ScoreManager.closeShaveBonus. Leave 0 to use the manager default.")]
    [SerializeField] private int customBonus = 0;

    [Tooltip("Label shown in floating text. e.g. \"Close Shave!\"")]
    [SerializeField] private string label = "Close Shave!";

    [SerializeField] private string playerTag = "Player";

    private bool fired;

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (fired) return;
        if (!other.CompareTag(playerTag)) return;
        if (ScoreManager.Instance == null) return;

        fired = true;
        int amount = customBonus > 0 ? customBonus : ScoreManager.Instance.closeShaveBonus;
        ScoreManager.Instance.AddBonus(amount, label);
    }
}
