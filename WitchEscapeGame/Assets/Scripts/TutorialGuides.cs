using UnityEngine;
using System.Collections;

/// <summary>
/// Coordinates the in-game tutorial gesture guides (Tap to Start, Tap to Fly,
/// Swipe to Throw) and shows them only for the player's first few runs.
///
/// After 'maxTutorialPlays' runs, the guides never appear again. The play count
/// persists across sessions via PlayerPrefs.
/// </summary>
public class TutorialGuides : MonoBehaviour
{
    [Header("Guide GameObjects")]
    public GameObject tapToStart;
    public GameObject tapToFly;
    public GameObject swipeToThrow;

    [Header("Timing")]
    [Tooltip("How long the 'keep tapping' (fly) guide stays after the run starts, " +
             "teaching the player to continue tapping after the first tap.")]
    public float flyGuideDuration = 4f;

    [Tooltip("Extra hold so the tapping cue lingers right after the first tap " +
             "before the fly guide takes over (player learns 'keep tapping').")]
    public float tapHoldAfterStart = 2f;

    public float swipeGuideDuration = 4f;

    [Header("First-Plays Gating")]
    [Tooltip("Show tutorial guides only for this many runs, then never again.")]
    public int maxTutorialPlays = 3;

    // PlayerPrefs key — keep consistent with your other keys.
    private const string PlayCountKey = "tutorial_play_count";

    private bool _showTutorialThisRun;

    void Start()
    {
        // Decide once at scene load whether tutorials run this time.
        int playsSoFar = PlayerPrefs.GetInt(PlayCountKey, 0);
        _showTutorialThisRun = playsSoFar < maxTutorialPlays;

        // Hide everything initially.
        if (tapToFly != null) tapToFly.SetActive(false);
        if (swipeToThrow != null) swipeToThrow.SetActive(false);

        // Only the "Tap to Start" guide shows pre-run, and only if we're still
        // within the tutorial window.
        if (tapToStart != null)
            tapToStart.SetActive(_showTutorialThisRun);
    }

    /// <summary>
    /// Wire this to GameStart.onGameStarted. Keeps the tapping cue visible a bit
    /// longer (so the player learns to KEEP tapping after the first tap), then
    /// plays the fly + swipe guides. Also increments the persistent play count.
    /// </summary>
    public void OnRunStarted()
    {
        if (!_showTutorialThisRun)
        {
            // Past the tutorial window — make sure the start guide is hidden.
            if (tapToStart != null) tapToStart.SetActive(false);
            return;
        }

        // Count this as a tutorial play and persist it.
        int playsSoFar = PlayerPrefs.GetInt(PlayCountKey, 0);
        PlayerPrefs.SetInt(PlayCountKey, playsSoFar + 1);
        PlayerPrefs.Save();

        StartCoroutine(GuideSequence());
    }

    private IEnumerator GuideSequence()
    {
        // Keep the tapping cue on screen for a short hold AFTER the first tap,
        // so the player understands they should keep tapping to fly.
        yield return new WaitForSeconds(tapHoldAfterStart);

        // Now hide the start guide and switch to the dedicated fly guide.
        if (tapToStart != null) tapToStart.SetActive(false);

        if (tapToFly != null) tapToFly.SetActive(true);
        yield return new WaitForSeconds(flyGuideDuration);
        if (tapToFly != null) tapToFly.SetActive(false);

        if (swipeToThrow != null) swipeToThrow.SetActive(true);
        yield return new WaitForSeconds(swipeGuideDuration);
        if (swipeToThrow != null) swipeToThrow.SetActive(false);
    }

    /// <summary>
    /// Optional: call from a debug menu or context menu to reset the tutorial
    /// so it shows again from scratch. Handy for testing.
    /// </summary>
    [ContextMenu("Reset Tutorial Play Count")]
    public void ResetTutorialProgress()
    {
        PlayerPrefs.DeleteKey(PlayCountKey);
        PlayerPrefs.Save();
        Debug.Log("[TutorialGuides] Play count reset — tutorials will show again.");
    }
}