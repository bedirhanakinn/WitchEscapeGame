using TMPro;
using UnityEngine;

/// <summary>
/// HUD coin counter. Hidden until first coin of the run is collected.
/// Auto-hides when GameManager fires onGameOver.
/// </summary>
[DefaultExecutionOrder(-50)]
public class CoinHudDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI countText;
    [Tooltip("Optional. If empty, the GameObject this script is on is toggled.")]
    [SerializeField] private GameObject rootToToggle;

    private bool subscribedTracker;
    private bool subscribedGameOver;

    void Awake()
    {
        if (rootToToggle == null) rootToToggle = gameObject;
        rootToToggle.SetActive(false);
        TrySubscribeTracker();
    }

    void Start()
    {
        TrySubscribeTracker();
        TrySubscribeGameOver();

        if (RunCoinTracker.Instance != null && RunCoinTracker.Instance.CoinsThisRun > 0)
            HandleChanged(RunCoinTracker.Instance.CoinsThisRun);
    }

    void OnDestroy()
    {
        if (subscribedTracker && RunCoinTracker.Instance != null)
            RunCoinTracker.Instance.OnRunCoinsChanged -= HandleChanged;
        if (subscribedGameOver && GameManager.instance != null)
            GameManager.instance.onGameOver.RemoveListener(HideOnGameOver);
    }

    private void TrySubscribeTracker()
    {
        if (subscribedTracker || RunCoinTracker.Instance == null) return;
        RunCoinTracker.Instance.OnRunCoinsChanged += HandleChanged;
        subscribedTracker = true;
    }

    private void TrySubscribeGameOver()
    {
        if (subscribedGameOver || GameManager.instance == null) return;
        GameManager.instance.onGameOver.AddListener(HideOnGameOver);
        subscribedGameOver = true;
    }

    private void HandleChanged(int count)
    {
        if (count <= 0)
        {
            rootToToggle.SetActive(false);
            return;
        }
        if (!rootToToggle.activeSelf) rootToToggle.SetActive(true);
        if (countText != null) countText.text = count.ToString();
    }

    private void HideOnGameOver()
    {
        rootToToggle.SetActive(false);
    }
}