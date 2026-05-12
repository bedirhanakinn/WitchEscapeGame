using System;
using UnityEngine;

/// <summary>
/// Tick-based score system. Adds points on a fixed interval (e.g. every 0.1s)
/// for an arcade "ticking number" feel. Bonuses can be added at any time.
/// High score persists to PlayerPrefs.
///
/// Pauses automatically when Time.timeScale == 0 (i.e. pause menu, game over).
/// Reset by scene reload (QuitRun → SceneManager.LoadScene).
/// </summary>
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    /// <summary>Fires with new score whenever it changes.</summary>
    public event Action<int> OnScoreChanged;

    /// <summary>Fires with bonus amount and reason whenever a bonus is awarded.</summary>
    public event Action<int, string> OnBonusAwarded;

    [Header("Tick scoring")]
    [Tooltip("Seconds between tick awards. Lower = smoother but smaller jumps.")]
    [SerializeField] private float tickInterval = 0.1f;

    [Tooltip("Points added per tick. With 0.1s interval, 5 pts/tick = 50 pts/sec.")]
    [SerializeField] private int pointsPerTick = 5;

    [Tooltip("If false, no tick scoring happens until StartScoring() is called.")]
    [SerializeField] private bool autoStart = false;

    [Header("Bonus defaults")]
    public int closeShaveBonus = 500;
    public int powerUpHitBonus = 200;

    [Header("Persistence")]
    [SerializeField] private string highScorePrefsKey = "score_high";

    public int CurrentScore { get; private set; }
    public int HighScore { get; private set; }
    public bool IsScoring { get; private set; }

    private float tickTimer;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        HighScore = PlayerPrefs.GetInt(highScorePrefsKey, 0);
    }

    void Start()
    {
        if (autoStart) StartScoring();
    }

    void Update()
    {
        if (!IsScoring) return;
        if (Time.timeScale == 0f) return; // paused / game over

        tickTimer += Time.deltaTime;
        while (tickTimer >= tickInterval)
        {
            tickTimer -= tickInterval;
            AddRaw(pointsPerTick);
        }
    }

    /// <summary>Begin tick scoring. Call when the run actually starts (e.g. from GameStart.onGameStarted).</summary>
    public void StartScoring()
    {
        IsScoring = true;
        tickTimer = 0f;
    }

    /// <summary>Stop tick scoring. Call on death or run end.</summary>
    public void StopScoring()
    {
        IsScoring = false;
    }

    /// <summary>Award a bonus with a label (e.g. "Close Shave", "Power-up Hit").</summary>
    public void AddBonus(int amount, string reason)
    {
        if (amount <= 0) return;
        AddRaw(amount);
        OnBonusAwarded?.Invoke(amount, reason);
    }

    private void AddRaw(int amount)
    {
        CurrentScore += amount;
        OnScoreChanged?.Invoke(CurrentScore);
    }

    /// <summary>
    /// Saves CurrentScore as new high score if it exceeds the previous one.
    /// Returns true if a new record was set.
    /// Call on game over (typically wired via GameManager.onGameOver).
    /// </summary>
    public bool CommitHighScore()
    {
        StopScoring();
        if (CurrentScore <= HighScore) return false;
        HighScore = CurrentScore;
        PlayerPrefs.SetInt(highScorePrefsKey, HighScore);
        PlayerPrefs.Save();
        return true;
    }

    // --- Debug helpers ---
    [ContextMenu("Debug: Add 1000 bonus")]
    private void DebugAdd1000() => AddBonus(1000, "Debug");

    [ContextMenu("Debug: Reset High Score")]
    private void DebugResetHigh()
    {
        HighScore = 0;
        PlayerPrefs.DeleteKey(highScorePrefsKey);
        PlayerPrefs.Save();
    }
}
