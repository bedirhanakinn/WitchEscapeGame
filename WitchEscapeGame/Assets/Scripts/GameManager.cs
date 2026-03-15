using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public GameObject gameOverPanel;

    void Awake()
    {
        instance = this;
    }

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
}