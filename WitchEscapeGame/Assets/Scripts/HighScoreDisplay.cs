using TMPro;
using UnityEngine;

/// <summary>
/// Shows the high score on the GameOver panel + a "NEW RECORD!" flash if beaten.
/// Wire RefreshDisplay() to the GameOver UIMenu's onShow event in the Inspector.
/// </summary>
public class HighScoreDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI currentScoreText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    [SerializeField] private GameObject newRecordBanner;

    [Tooltip("{0} = current run score")]
    [SerializeField] private string currentFormat = "Score: {0:N0}";

    [Tooltip("{0} = high score")]
    [SerializeField] private string highFormat = "Best: {0:N0}";

    void Awake()
    {
        if (newRecordBanner != null) newRecordBanner.SetActive(false);
    }

    /// <summary>Wire this to GameOver UIMenu's onShow UnityEvent.</summary>
    public void RefreshDisplay()
    {
        if (ScoreManager.Instance == null) return;

        bool newRecord = ScoreManager.Instance.CommitHighScore();

        if (currentScoreText != null)
            currentScoreText.text = string.Format(currentFormat, ScoreManager.Instance.CurrentScore);
        if (highScoreText != null)
            highScoreText.text = string.Format(highFormat, ScoreManager.Instance.HighScore);
        if (newRecordBanner != null)
            newRecordBanner.SetActive(newRecord);
    }
}
