using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The death-screen button layer for the revive system. Lives on (or under)
/// the GameOver panel. Knows about the "Watch Ad" button ONLY.
///
/// The 3-2-1 countdown is handled separately by ReviveCountdownUI, which lives
/// OUTSIDE the GameOver panel so it survives the panel being closed on revive.
/// This separation is the whole point of the bug fix: button and countdown are
/// different UI concerns living in different parts of the hierarchy.
/// </summary>
public class ReviveButtonUI : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The 'Watch Ad to Revive' button.")]
    [SerializeField] private Button watchAdButton;

    void OnEnable()
    {
        // Decide button visibility every time the GameOver panel appears.
        RefreshButton();

        if (ReviveController.Instance != null)
        {
            // Re-enable the button if a revive attempt was declined, so the
            // visual is consistent next time the panel shows.
            ReviveController.Instance.OnReviveUnavailableOrDeclined += HandleReviveDeclined;
        }
    }

    void OnDisable()
    {
        if (ReviveController.Instance != null)
        {
            ReviveController.Instance.OnReviveUnavailableOrDeclined -= HandleReviveDeclined;
        }
    }

    /// <summary>
    /// Shows or hides the watch-ad button based on whether a revive can be offered.
    /// Also safe to wire to the GameOver UIMenu's onShow event.
    /// </summary>
    public void RefreshButton()
    {
        bool canRevive = ReviveController.Instance != null &&
                         ReviveController.Instance.CanOfferRevive;

        if (watchAdButton != null)
        {
            watchAdButton.gameObject.SetActive(canRevive);
            watchAdButton.interactable = true;
        }
    }

    /// <summary>
    /// Hook this to the watch-ad Button's onClick in the Inspector.
    /// </summary>
    public void OnWatchAdPressed()
    {
        // Disable immediately to prevent double taps.
        if (watchAdButton != null) watchAdButton.interactable = false;

        ReviveController.Instance?.RequestRevive();
    }

    private void HandleReviveDeclined()
    {
        // Re-enable for visual consistency; normal Game Over flow takes over.
        if (watchAdButton != null) watchAdButton.interactable = true;
    }
}