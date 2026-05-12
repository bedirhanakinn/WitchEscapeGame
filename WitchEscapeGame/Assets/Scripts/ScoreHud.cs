using TMPro;
using UnityEngine;

/// <summary>
/// HUD score display. Hidden at start; fades in once score crosses a threshold.
/// Fades out on game over.
///
/// Uses UIFader for the animated transitions. If no UIFader is wired, falls back
/// to plain SetActive toggling.
///
/// Score is tied to gameplay time (no scoring while paused), so a threshold of 350
/// with 50 pts/sec base = ~7 seconds of actual play.
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

    private bool subscribedScore;
    private bool subscribedGameOver;
    private bool revealed;

    void Awake()
    {
        if (fader == null) fader = GetComponent<UIFader>();

        // If threshold is 0, reveal immediately
        if (revealAtScore <= 0)
        {
            revealed = true;
            ApplyShow(instant: true);
        }
        else
        {
            ApplyHide(instant: true);
        }

        TrySubscribeScore();
    }

    void Start()
    {
        TrySubscribeScore();
        TrySubscribeGameOver();
        Refresh(ScoreManager.Instance != null ? ScoreManager.Instance.CurrentScore : 0);
    }

    void OnDestroy()
    {
        if (subscribedScore && ScoreManager.Instance != null)
            ScoreManager.Instance.OnScoreChanged -= Refresh;
        if (subscribedGameOver && GameManager.instance != null)
            GameManager.instance.onGameOver.RemoveListener(HideOnGameOver);
    }

    private void TrySubscribeScore()
    {
        if (subscribedScore || ScoreManager.Instance == null) return;
        ScoreManager.Instance.OnScoreChanged += Refresh;
        subscribedScore = true;
    }

    private void TrySubscribeGameOver()
    {
        if (subscribedGameOver || GameManager.instance == null) return;
        GameManager.instance.onGameOver.AddListener(HideOnGameOver);
        subscribedGameOver = true;
    }

    private void Refresh(int score)
    {
        if (!revealed && score >= revealAtScore)
        {
            revealed = true;
            ApplyShow(instant: false);
        }

        if (scoreText != null) scoreText.text = string.Format(format, score);
    }

    private void HideOnGameOver()
    {
        ApplyHide(instant: false);
    }

    private void ApplyShow(bool instant)
    {
        if (fader != null)
        {
            if (instant)
            {
                // Manual instant set since UIFader.Show is animated
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