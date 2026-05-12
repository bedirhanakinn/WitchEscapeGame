using TMPro;
using UnityEngine;

/// <summary>
/// HUD score display with count-up animation on bonuses.
///
/// - Base ticks (small +5 increments from time scoring) snap immediately to the new value.
/// - Bonuses (Close Shave, Power-up, anything via ScoreManager.AddBonus) animate
///   the displayed number rolling up to the target.
///
/// Hidden at start; reveals once score crosses a threshold. Hides on game over.
/// Uses UIFader for animated reveal/hide if present.
/// </summary>
[DefaultExecutionOrder(-50)]
public class ScoreHud : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;

    [Tooltip("UIFader for animated reveal/hide. If empty, falls back to SetActive on the GameObject.")]
    [SerializeField] private UIFader fader;

    [Tooltip("Format string. {0} is the score. Use 'N0' for thousands separators.")]
    [SerializeField] private string format = "{0:N0}";

    [Header("Reveal")]
    [Tooltip("Score must reach this value before the HUD appears. Set 0 to always show.")]
    [SerializeField] private int revealAtScore = 350;

    [Header("Count-up animation")]
    [Tooltip("How fast the display catches up to a bonus in points per second.")]
    [SerializeField] private float countUpRate = 1000f;

    [Tooltip("Maximum time a single count-up animation can take. Caps very large bonuses.")]
    [SerializeField] private float countUpMaxDuration = 0.8f;

    [Tooltip("Bonuses smaller than this snap immediately (no animation).")]
    [SerializeField] private int countUpMinDelta = 50;

    private bool subscribedScore;
    private bool subscribedBonus;
    private bool subscribedGameOver;
    private bool revealed;

    private int targetScore;
    private float displayedScore;
    private float catchUpVelocity;

    void Awake()
    {
        if (fader == null) fader = GetComponent<UIFader>();

        if (revealAtScore <= 0)
        {
            revealed = true;
            ApplyShow(instant: true);
        }
        else
        {
            ApplyHide(instant: true);
        }

        TrySubscribe();
    }

    void Start()
    {
        TrySubscribe();
        TrySubscribeGameOver();
        int initial = ScoreManager.Instance != null ? ScoreManager.Instance.CurrentScore : 0;
        targetScore = initial;
        displayedScore = initial;
        RenderText(initial);
    }

    void OnDestroy()
    {
        if (subscribedScore && ScoreManager.Instance != null)
            ScoreManager.Instance.OnScoreChanged -= HandleScoreChanged;
        if (subscribedBonus && ScoreManager.Instance != null)
            ScoreManager.Instance.OnBonusAwarded -= HandleBonusAwarded;
        if (subscribedGameOver && GameManager.instance != null)
            GameManager.instance.onGameOver.RemoveListener(HideOnGameOver);
    }

    private void TrySubscribe()
    {
        if (ScoreManager.Instance == null) return;
        if (!subscribedScore)
        {
            ScoreManager.Instance.OnScoreChanged += HandleScoreChanged;
            subscribedScore = true;
        }
        if (!subscribedBonus)
        {
            ScoreManager.Instance.OnBonusAwarded += HandleBonusAwarded;
            subscribedBonus = true;
        }
    }

    private void TrySubscribeGameOver()
    {
        if (subscribedGameOver || GameManager.instance == null) return;
        GameManager.instance.onGameOver.AddListener(HideOnGameOver);
        subscribedGameOver = true;
    }

    // Bonus arrives BEFORE OnScoreChanged for the same delta. We use it to detect
    // "this next score change should animate." After the animation flag is consumed,
    // subsequent score changes snap.
    private bool nextChangeIsBonus;
    private int pendingBonusAmount;

    private void HandleBonusAwarded(int amount, string reason)
    {
        if (amount >= countUpMinDelta)
        {
            nextChangeIsBonus = true;
            pendingBonusAmount = amount;
        }
    }

    private void HandleScoreChanged(int newScore)
    {
        // Reveal trigger
        if (!revealed && newScore >= revealAtScore)
        {
            revealed = true;
            ApplyShow(instant: false);
        }

        targetScore = newScore;

        if (nextChangeIsBonus)
        {
            // Compute catch-up velocity for this animation
            float delta = pendingBonusAmount;
            float duration = Mathf.Min(delta / countUpRate, countUpMaxDuration);
            duration = Mathf.Max(duration, 0.05f); // never zero
            catchUpVelocity = delta / duration;
            nextChangeIsBonus = false;
            pendingBonusAmount = 0;
            // displayedScore stays where it is; Update will tween it up
        }
        else
        {
            // Base tick — snap
            displayedScore = newScore;
            catchUpVelocity = 0f;
            RenderText(newScore);
        }
    }

    void Update()
    {
        if (catchUpVelocity <= 0f) return;
        if (Mathf.RoundToInt(displayedScore) >= targetScore)
        {
            displayedScore = targetScore;
            catchUpVelocity = 0f;
            RenderText(targetScore);
            return;
        }

        // Unscaled so the animation completes even if a pause hits mid-roll
        displayedScore += catchUpVelocity * Time.unscaledDeltaTime;
        if (displayedScore >= targetScore)
        {
            displayedScore = targetScore;
            catchUpVelocity = 0f;
        }
        RenderText(Mathf.RoundToInt(displayedScore));
    }

    private void RenderText(int value)
    {
        if (scoreText != null) scoreText.text = string.Format(format, value);
    }

    private void HideOnGameOver()
    {
        // If a count-up was in progress, snap to final before hiding
        displayedScore = targetScore;
        catchUpVelocity = 0f;
        RenderText(targetScore);
        ApplyHide(instant: false);
    }

    private void ApplyShow(bool instant)
    {
        if (fader != null)
        {
            if (instant)
            {
                var cg = fader.GetComponent<CanvasGroup>();
                if (cg != null) { cg.alpha = 0f; cg.interactable = false; cg.blocksRaycasts = false; }
            }
            else fader.Show();
        }
        else
        {
            gameObject.SetActive(true);
        }
    }

    private void ApplyHide(bool instant)
    {
        if (fader != null)
        {
            if (instant)
            {
                var cg = fader.GetComponent<CanvasGroup>();
                if (cg != null) { cg.alpha = 0f; cg.interactable = false; cg.blocksRaycasts = false; }
            }
            else fader.Hide();
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}