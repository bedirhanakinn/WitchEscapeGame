using UnityEngine;
using TMPro; // Needed for the countdown text
using System.Collections;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Original References")]
    public GameObject gameOverPanel;

    [Header("Pause Menu References")]
    public GameObject pauseMenuPanel;
    public TextMeshProUGUI countdownText;

    void Awake()
    {
        instance = this;
    }

    // --- ORIGINAL LOGIC ---
    public void GameOver()
    {
        Time.timeScale = 0;
        gameOverPanel.SetActive(true);
    }

    public void Resume()
    {
        Time.timeScale = 1;
        gameOverPanel.SetActive(false);
    }

    // --- NEW PAUSE LOGIC ---
    public void TogglePause(bool isPaused)
    {
        if (isPaused)
        {
            Time.timeScale = 0f;
            pauseMenuPanel.SetActive(true);
        }
        else
        {
            // This starts the 3s countdown sequence
            StartCoroutine(ResumeSequence());
        }
    }

    private IEnumerator ResumeSequence()
    {
        pauseMenuPanel.SetActive(false);
        
        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
            for (int i = 3; i > 0; i--)
            {
                countdownText.text = i.ToString();
                yield return new WaitForSecondsRealtime(1f); 
            }
            countdownText.gameObject.SetActive(false);
        }
        else
        {
            yield return new WaitForSecondsRealtime(3f);
        }

        Time.timeScale = 1f;
    }

    public void QuitRun()
    {
        Time.timeScale = 1f;
        // Reloads the scene to go back to the absolute starting state
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}