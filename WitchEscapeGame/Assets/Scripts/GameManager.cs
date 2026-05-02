using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Single source of truth for game state (running / paused / game-over).
/// All menu visuals are delegated to UIManager — this class only handles
/// timescale, the resume countdown, and scene reloads.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Resume Countdown")]
    [Tooltip("TextMeshPro element used to display the 3-2-1 countdown when resuming from pause.")]
    public TextMeshProUGUI countdownText;

    [Tooltip("How long to count down before un-pausing.")]
    public int countdownSeconds = 3;

    public bool IsPaused { get; private set; }
    public bool IsGameOver { get; private set; }

    void Awake()
    {
        instance = this;
    }

    // ---------------------------------------------------------------
    // GAME OVER
    // ---------------------------------------------------------------

    public void GameOver()
    {
        if (IsGameOver) return;
        IsGameOver = true;

        Time.timeScale = 0f;

        if (UIManager.Instance != null)
            UIManager.Instance.Open(UIManager.MenuId.GameOver);
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