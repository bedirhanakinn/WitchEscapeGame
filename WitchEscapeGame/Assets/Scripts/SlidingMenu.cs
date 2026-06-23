using UnityEngine;
using System.Collections;

/// <summary>
/// Slides a menu container vertically between a "shown" and "hidden" position
/// when the up/down arrow buttons are pressed.
///
/// Attach to the MenuContainer (the panel that holds Shop/Settings/Credits).
/// Wire the two arrow buttons' onClick to ShowMenu() and HideMenu().
///
/// Movement is driven by RectTransform.anchoredPosition, animated with an
/// unscaled-time lerp so it works on menus regardless of Time.timeScale.
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class SlidingMenu : MonoBehaviour
{
    [Header("Positions (anchored Y)")]
    [Tooltip("Y position when the menu is fully visible on screen.")]
    [SerializeField] private float shownY = 0f;

    [Tooltip("Y position when the menu is slid away off-screen. " +
             "For a slide-down-to-hide, this is a large negative number.")]
    [SerializeField] private float hiddenY = -1200f;

    [Header("Animation")]
    [Tooltip("Seconds the slide takes.")]
    [SerializeField] private float slideDuration = 0.35f;

    [Tooltip("Eased motion (smoothstep) vs linear.")]
    [SerializeField] private bool useEasing = true;

    [Header("Start State")]
    [Tooltip("If true, the menu starts shown. If false, starts hidden.")]
    [SerializeField] private bool startShown = true;

    [Header("Arrow Buttons (optional auto-hide)")]
    [Tooltip("The 'up' arrow that shows the menu. Hidden when menu is already shown.")]
    [SerializeField] private GameObject upArrow;
    [Tooltip("The 'down' arrow that hides the menu. Hidden when menu is already hidden.")]
    [SerializeField] private GameObject downArrow;

    private RectTransform _rt;
    private Coroutine _slideRoutine;
    private bool _isShown;

    void Awake()
    {
        _rt = GetComponent<RectTransform>();
    }

    void Start()
    {
        // Snap to the initial state without animating.
        _isShown = startShown;
        Vector2 pos = _rt.anchoredPosition;
        pos.y = _isShown ? shownY : hiddenY;
        _rt.anchoredPosition = pos;
        UpdateArrowVisibility();
    }

    /// <summary>
    /// Hides both arrow buttons. Wire this to GameStart.onGameStarted so the
    /// arrows disappear when the player starts a run.
    /// </summary>
    public void HideArrows()
    {
        if (upArrow != null) upArrow.SetActive(false);
        if (downArrow != null) downArrow.SetActive(false);
    }

    /// <summary>Wire the UP arrow button's onClick here.</summary>
    public void ShowMenu()
    {
        if (_isShown) return;
        _isShown = true;
        StartSlide(shownY);
    }

    /// <summary>Wire the DOWN arrow button's onClick here.</summary>
    public void HideMenu()
    {
        if (!_isShown) return;
        _isShown = false;
        StartSlide(hiddenY);
    }

    /// <summary>Toggle — handy if you ever want a single button.</summary>
    public void ToggleMenu()
    {
        if (_isShown) HideMenu();
        else ShowMenu();
    }

    private void StartSlide(float targetY)
    {
        if (_slideRoutine != null) StopCoroutine(_slideRoutine);
        _slideRoutine = StartCoroutine(SlideTo(targetY));
        // Arrow swap is delayed to end of slide — see SlideTo below.
    }

    private IEnumerator SlideTo(float targetY)
    {
        // Hide BOTH arrows at the start of the slide so neither flickers
        // or disappears awkwardly mid-animation.
        if (upArrow != null) upArrow.SetActive(false);
        if (downArrow != null) downArrow.SetActive(false);

        Vector2 start = _rt.anchoredPosition;
        float startY = start.y;
        float elapsed = 0f;

        while (elapsed < slideDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / slideDuration);
            if (useEasing) t = t * t * (3f - 2f * t); // smoothstep

            Vector2 pos = _rt.anchoredPosition;
            pos.y = Mathf.Lerp(startY, targetY, t);
            _rt.anchoredPosition = pos;
            yield return null;
        }

        Vector2 final = _rt.anchoredPosition;
        final.y = targetY;
        _rt.anchoredPosition = final;
        _slideRoutine = null;

        // NOW show the correct arrow for the new state.
        UpdateArrowVisibility();
    }

    /// <summary>
    /// Shows the relevant arrow for the current state (up arrow when hidden,
    /// down arrow when shown). Optional — only runs if arrows are assigned.
    /// </summary>
    private void UpdateArrowVisibility()
    {
        if (upArrow != null) upArrow.SetActive(!_isShown);
        if (downArrow != null) downArrow.SetActive(_isShown);
    }
}