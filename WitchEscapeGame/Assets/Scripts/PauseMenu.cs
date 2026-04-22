using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class PauseManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject pauseMenuPanel;
    public TextMeshProUGUI countdownText;

    [Header("Settings State")]
    private bool vibEnabled = true;
    private bool musicEnabled = true;
    private bool sfxEnabled = true;

    // 1. OPEN PAUSE MENU
    public void PressPause()
    {
        Time.timeScale = 0f; // Freeze game
        pauseMenuPanel.SetActive(true);
    }

    // 2. RESUME (WITH COUNTDOWN)
    public void PressResume()
    {
        pauseMenuPanel.SetActive(false);
        StartCoroutine(ResumeCoroutine());
    }

    IEnumerator ResumeCoroutine()
    {
        countdownText.gameObject.SetActive(true);
        
        for (int i = 3; i > 0; i--)
        {
            countdownText.text = i.ToString();
            yield return new WaitForSecondsRealtime(1f); // Realtime ignores the 0 timescale
        }

        countdownText.gameObject.SetActive(false);
        Time.timeScale = 1f; // Unfreeze game
    }

    // 3. QUIT RUN
    public void PressQuit()
    {
        Time.timeScale = 1f;
        // Reloads the scene to go back to the starting "Tap to Play" state
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // 4. TOGGLE SETTINGS (Stubs for your logic)
    public void ToggleVibration()
    {
        vibEnabled = !vibEnabled;
        if(vibEnabled) Handheld.Vibrate();
        Debug.Log("Vibration is now: " + vibEnabled);
    }

    public void ToggleMusic()
    {
        musicEnabled = !musicEnabled;
        // Add music mute logic here later
        Debug.Log("Music is now: " + musicEnabled);
    }

    public void ToggleSFX()
    {
        sfxEnabled = !sfxEnabled;
        // Add SFX mute logic here later
        Debug.Log("SFX is now: " + sfxEnabled);
    }
}