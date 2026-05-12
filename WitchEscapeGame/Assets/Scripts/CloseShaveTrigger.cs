using System.Collections;
using UnityEngine;

/// <summary>
/// Pattern B (lenient): bonus is awarded only if the game is still active.
///
/// Final implementation uses Time.timeScale as the source of truth.
/// GameManager.GameOver() sets Time.timeScale = 0 synchronously on death.
/// By the time WaitForEndOfFrame resolves, timeScale reflects the death state
/// regardless of event order. This is more reliable than any flag.
///
/// - Player ENTERS the zone -> arm the bonus.
/// - Player EXITS the zone -> wait until end of frame -> award only if timeScale > 0.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class CloseShaveTrigger : MonoBehaviour
{
    [Tooltip("Override the default ScoreManager.closeShaveBonus. Leave 0 to use the manager default.")]
    [SerializeField] private int customBonus = 0;

    [Tooltip("Label shown in floating text.")]
    [SerializeField] private string label = "Close Shave!";

    [SerializeField] private string playerTag = "Player";

    private bool armed;
    private bool fired;
    private bool checkPending;

    void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (fired) return;
        if (!other.CompareTag(playerTag)) return;
        armed = true;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (fired) return;
        if (!armed) return;
        if (checkPending) return;
        if (!other.CompareTag(playerTag)) return;

        checkPending = true;
        StartCoroutine(AwardAfterFrameCheck());
    }

    IEnumerator AwardAfterFrameCheck()
    {
        // Wait until end of frame so any death logic from this frame's collisions
        // has fully processed (including GameManager.GameOver() which sets timeScale=0).
        yield return new WaitForEndOfFrame();

        checkPending = false;

        if (fired) yield break;
        if (ScoreManager.Instance == null) yield break;

        // Time.timeScale is set to 0 by GameManager.GameOver() and GameManager.Pause().
        // If it's zero, the run is not in a state where bonuses should be awarded.
        if (Time.timeScale <= 0f) yield break;

        // Belt and suspenders: also check the flag in case timeScale was restored.
        if (GameManager.instance != null && GameManager.instance.IsGameOver) yield break;

        fired = true;
        int amount = customBonus > 0 ? customBonus : ScoreManager.Instance.closeShaveBonus;
        ScoreManager.Instance.AddBonus(amount, label);
    }
}