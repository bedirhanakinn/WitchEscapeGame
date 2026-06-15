using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

/// <summary>
/// Single source of truth for game state (running / paused / game-over).
/// All menu visuals are delegated to UIManager — this class only handles
/// timescale, the resume countdown, scene reloads, and broadcasting state events.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Resume Countdown")]
    [Tooltip("TextMeshPro element used to display the 3-2-1 countdown when resuming from pause.")]
    public TextMeshProUGUI countdownText;

    [Tooltip("How long to count down before un-pausing.")]
    public int countdownSeconds = 3;

    [Header("Events")]
    [Tooltip("Fired when the player dies. Wire HUD UIFaders here so they fade out.")]
    public UnityEvent onGameOver;

    // Revive event — invoke this after a successful revive to re-show HUD / pause button, etc.
    public UnityEvent onRevive;

    // Reference to your player controller so we can restore control after a revive.
    // Assign in inspector or set from code.
    public PlayerController playerController;

    public bool IsPaused { get; private set; }
    public bool IsGameOver { get; private set; }

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        // Subscribe to ReviveController once all Awake() have run (safer than Awake).
        if (ReviveController.Instance != null)
        {
            ReviveController.Instance.OnReviveCountdownComplete += ResumeFromRevive;
            ReviveController.Instance.OnAdRewardGranted += HideGameOverForRevive;  // ADD THIS
        }
    }

    void OnDestroy()
    {
        if (ReviveController.Instance != null)
        {
            ReviveController.Instance.OnReviveCountdownComplete -= ResumeFromRevive;
            ReviveController.Instance.OnAdRewardGranted -= HideGameOverForRevive;  // ADD THIS
        }
    }

    // ---------------------------------------------------------------
    // GAME OVER
    // ---------------------------------------------------------------

    public void GameOver()
    {
        if (IsGameOver) return;
        ///Debug.Log("GameOver called");
        IsGameOver = true;

        Time.timeScale = 0f;

        if (UIManager.Instance != null)
            UIManager.Instance.Open(UIManager.MenuId.GameOver);

        // Notify listeners (HUD UIFaders, etc.) that the game has ended.
        onGameOver?.Invoke();
    }

    // ---------------------------------------------------------------
    // PAUSE / RESUME
    // ---------------------------------------------------------------

    /// <summary>Wire the pause button's onClick directly to this.</summary>
    public void Pause()
    {
        if (IsPaused || IsGameOver) return;
        IsPaused = true;

        Time.timeScale = 0f;

        if (UIManager.Instance != null)
            UIManager.Instance.Open(UIManager.MenuId.Pause);
    }

    /// <summary>Wire the Resume button's onClick directly to this.</summary>
    public void Resume()
    {
        if (!IsPaused) return;

        // Close every menu (the pause panel and anything opened from it, e.g. Settings).
        if (UIManager.Instance != null)
            UIManager.Instance.CloseAll();

        StartCoroutine(ResumeCountdown());
    }

    IEnumerator ResumeCountdown()
    {
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
            for (int i = countdownSeconds; i > 0; i--)
            {
                countdownText.text = i.ToString();
                yield return new WaitForSecondsRealtime(1f);
            }
            countdownText.gameObject.SetActive(false);
        }
        else
        {
            yield return new WaitForSecondsRealtime(countdownSeconds);
        }

        Time.timeScale = 1f;
        IsPaused = false;
    }

    /// <summary>
    /// Hides the GameOver menu immediately when the revive ad reward is granted,
    /// so only the countdown is visible. Does NOT resume gameplay — that happens
    /// later in ResumeFromRevive when the countdown completes.
    /// </summary>
    public void HideGameOverForRevive()
    {
        if (UIManager.Instance != null)
            UIManager.Instance.CloseAll();
    }

    /// <summary>
    /// Called when the revive countdown finishes. Restores control to the player
    /// and continues the SAME run (no scene reload).
    ///
    /// ENDLESS RUNNER NOTE:
    /// Because platforms scroll and despawn, we can't perfectly freeze world state.
    /// The standard runner approach is to (a) clear the obstacle that killed the
    /// player and any immediate nearby hazards, and (b) give brief invulnerability.
    /// Implement ClearNearbyHazards() / grant i-frames to taste.
    /// </summary>
    public void ResumeFromRevive()
    {
        IsGameOver = false;

        // Clear whatever killed the player so they don't instantly die again.
        ClearNearbyHazards();

        // Put the player back into a controllable, alive state.
        // Adjust to match your PlayerController's API.
        if (playerController != null)
        {
            playerController.ReviveToNormalState();
        }

        // Resume time. If you want a brief grace period, start an i-frame coroutine here.
        Time.timeScale = 1f;

        // Re-show the pause button HUD etc. via your existing events if needed.
        onRevive?.Invoke();
    }

    /// <summary>
    /// Destroy/disable obstacles near the player so the revive isn't instantly fatal.
    /// Implementation depends on your obstacle tags/layers.
    /// </summary>
    private void ClearNearbyHazards()
    {
        if (playerController == null) return;

        Vector2 center = playerController.transform.position;
        float clearRadius = 8f;
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, clearRadius);
        foreach (var h in hits)
        {
            if (h.CompareTag("Death") || h.CompareTag("Stumble"))
                h.gameObject.SetActive(false);
        }
    }

    // ---------------------------------------------------------------
    // QUIT / RESTART
    // ---------------------------------------------------------------

    /// <summary>Reloads the scene. Wire to Quit-to-Main-Menu and Game-Over Retry buttons.</summary>
    public void QuitRun()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}