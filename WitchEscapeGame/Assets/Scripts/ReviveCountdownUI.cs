using UnityEngine;
using TMPro;

/// <summary>
/// Dedicated overlay for the 3-2-1 revive countdown.
///
/// CRITICAL: This component must live on a GameObject that is NOT a child of
/// the GameOver panel (or any UIMenu that UIManager.CloseAll() hides). Put it
/// directly under the Canvas as its own object so it survives the GameOver
/// panel being closed during ResumeFromRevive().
///
/// It listens to ReviveController's countdown events and shows/updates/hides
/// the big number itself. It owns nothing about the death screen button.
/// </summary>
public class ReviveCountdownUI : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The big TMP number shown during the 3-2-1 countdown.")]
    [SerializeField] private TMP_Text countdownText;

    [Tooltip("Root object to enable/disable for the whole countdown overlay. " +
             "If left empty, this GameObject is used.")]
    [SerializeField] private GameObject overlayRoot;

    void Awake()
    {
        // Default the overlay root to this object if not assigned.
        if (overlayRoot == null) overlayRoot = gameObject;

        // Start hidden.
        overlayRoot.SetActive(false);
    }

    void OnEnable()
    {
        // Subscribe whenever this object becomes active. Because the overlay
        // starts disabled, we subscribe in Start instead (see below) to be safe.
    }

    void Start()
    {
        Subscribe();
    }

    void OnDestroy()
    {
        Unsubscribe();
    }

    private void Subscribe()
    {
        if (ReviveController.Instance == null) return;
        ReviveController.Instance.OnReviveCountdownTick += HandleTick;
        ReviveController.Instance.OnReviveCountdownComplete += HandleComplete;
    }

    private void Unsubscribe()
    {
        if (ReviveController.Instance == null) return;
        ReviveController.Instance.OnReviveCountdownTick -= HandleTick;
        ReviveController.Instance.OnReviveCountdownComplete -= HandleComplete;
    }

    private void HandleTick(int number)
    {
        // Make sure the overlay is visible, then show the current number.
        overlayRoot.SetActive(true);

        if (countdownText != null)
        {
            countdownText.gameObject.SetActive(true);
            countdownText.text = number.ToString();
        }
    }

    private void HandleComplete()
    {
        // Countdown finished — hide the overlay. Gameplay resume is handled
        // by GameManager.ResumeFromRevive (also subscribed to OnReviveCountdownComplete).
        overlayRoot.SetActive(false);
    }
}
