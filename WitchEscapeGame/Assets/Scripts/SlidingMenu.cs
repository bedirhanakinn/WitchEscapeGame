using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Slides a cloud menu panel vertically between a shown and hidden position.
/// A SINGLE toggle button (child of the panel, so it moves with it) flips its
/// icon between a down-chevron (pull menu down) and up-chevron (push menu up).
///
/// When hidden, the panel sits above the screen with only the toggle button
/// peeking down below the top edge, so the player can pull it back down.
///
/// Attach to the cloud panel (the RectTransform that holds the 3 buttons + toggle).
/// Wire the toggle button's onClick to Toggle().
/// </summary>
[RequireComponent(typeof(RectTransform))]
public class SlidingMenu : MonoBehaviour
{
    [Header("Positions (anchored Y)")]
    [Tooltip("Y when the menu is fully visible (pulled down).")]
    [SerializeField] private float shownY = 0f;

    [Tooltip("Y when hidden (pushed up). Positive pushes the panel above the screen, " +
             "leaving only the toggle button peeking below the top edge.")]
    [SerializeField] private float hiddenY = 1000f;

    [Header("Animation")]
    [SerializeField] private float slideDuration = 0.35f;
    [SerializeField] private bool useEasing = true;

    [Header("Start State")]
    [Tooltip("If true the menu starts shown (pulled down).")]
    [SerializeField] private bool startShown = true;

    [Header("Toggle Button Icon")]
    [Tooltip("The Image on the toggle button whose sprite we swap.")]
    [SerializeField] private Image toggleIcon;
    [Tooltip("Icon shown when the menu is SHOWN (press to push it up).")]
    [SerializeField] private Sprite upIconSprite;
    [Tooltip("Icon shown when the menu is HIDDEN (press to pull it down).")]
    [SerializeField] private Sprite downIconSprite;

    [Header("Toggle Button (hide on run start)")]
    [Tooltip("The whole toggle button GameObject — hidden when a run starts.")]
    [SerializeField] private GameObject toggleButton;

    private RectTransform _rt;
    private Coroutine _slideRoutine;
    private bool _isShown;
    private bool _gameStarted;

    void Awake()
    {
        _rt = GetComponent<RectTransform>();
    }

    void Start()
    {
        _isShown = startShown;
        Vector2 pos = _rt.anchoredPosition;
        pos.y = _isShown ? shownY : hiddenY;
        _rt.anchoredPosition = pos;
        UpdateToggleIcon();
    }

    /// <summary>Wire the toggle button's onClick to this single method.</summary>
    public void Toggle()
    {
        if (_isShown) Hide();
        else Show();
    }

    public void Show()
    {
        if (_isShown) return;
        _isShown = true;
        StartSlide(shownY);
    }

    public void Hide()
    {
        if (!_isShown) return;
        _isShown = false;
        StartSlide(hiddenY);
    }

    private void StartSlide(float targetY)
    {
        if (_slideRoutine != null) StopCoroutine(_slideRoutine);
        _slideRoutine = StartCoroutine(SlideTo(targetY));
    }

    private IEnumerator SlideTo(float targetY)
    {
        Vector2 start = _rt.anchoredPosition;
        float startY = start.y;
        float elapsed = 0f;

        while (elapsed < slideDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / slideDuration);
            if (useEasing) t = t * t * (3f - 2f * t);

            Vector2 pos = _rt.anchoredPosition;
            pos.y = Mathf.Lerp(startY, targetY, t);
            _rt.anchoredPosition = pos;
            yield return null;
        }

        Vector2 final = _rt.anchoredPosition;
        final.y = targetY;
        _rt.anchoredPosition = final;
        _slideRoutine = null;

        // Flip the icon at the end of the slide so it matches the new state.
        UpdateToggleIcon();
    }

    /// <summary>
    /// Sets the toggle icon: up-chevron while shown (press to push up),
    /// down-chevron while hidden (press to pull down).
    /// </summary>
    private void UpdateToggleIcon()
    {
        if (toggleIcon == null) return;
        if (_isShown && upIconSprite != null) toggleIcon.sprite = upIconSprite;
        else if (!_isShown && downIconSprite != null) toggleIcon.sprite = downIconSprite;
    }

    /// <summary>
    /// Wire to GameStart.onGameStarted. Hides the toggle button so it doesn't
    /// linger on screen during gameplay.
    /// </summary>
    public void HideArrows()
    {
        _gameStarted = true;
        if (toggleButton != null) toggleButton.SetActive(false);
    }
}