using System.Collections;
using UnityEngine;

/// <summary>
/// Simple CanvasGroup-based fader for HUD elements like the pause button,
/// score display, etc. Not part of the UIManager menu stack — use this for
/// in-game UI that should appear/disappear based on game state events.
///
/// The GameObject stays active throughout; only alpha and raycast blocking change.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class UIFader : MonoBehaviour
{
    [Header("Fade")]
    public float fadeDuration = 0.25f;

    [Header("Initial State")]
    [Tooltip("If true, this element starts invisible and non-interactive when the scene loads.")]
    public bool startHidden = true;

    private CanvasGroup canvasGroup;
    private Coroutine fadeRoutine;

    public bool IsVisible { get; private set; }

    private CanvasGroup CG
    {
        get
        {
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
            return canvasGroup;
        }
    }

    void Awake()
    {
        if (startHidden)
        {
            CG.alpha = 0f;
            CG.interactable = false;
            CG.blocksRaycasts = false;
            IsVisible = false;
        }
        else
        {
            CG.alpha = 1f;
            CG.interactable = true;
            CG.blocksRaycasts = true;
            IsVisible = true;
        }
    }

    /// <summary>Fade in. Wire to a Button.onClick or UnityEvent.</summary>
    public void Show()
    {
        if (IsVisible) return;
        IsVisible = true;
        StartFade(1f);
    }

    /// <summary>Fade out. Wire to a Button.onClick or UnityEvent.</summary>
    public void Hide()
    {
        if (!IsVisible) return;
        // Guard against coroutine being started on an inactive GameObject
        // (can happen when CloseAll fires onHide on already-hidden panels)
        if (!gameObject.activeInHierarchy)
        {
            CG.alpha = 0f;
            CG.interactable = false;
            CG.blocksRaycasts = false;
            IsVisible = false;
            return;
        }
        IsVisible = false;
        StartFade(0f);
    }

    void StartFade(float target)
    {
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeRoutine(target));
    }

    IEnumerator FadeRoutine(float target)
    {
        // Block taps the moment we start hiding so users can't click during fade-out.
        bool becomingInteractive = target > 0f;
        CG.interactable = becomingInteractive;
        CG.blocksRaycasts = becomingInteractive;

        float start = CG.alpha;
        float t = 0f;

        // Use unscaled time so the fade still plays when the game is paused.
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            CG.alpha = Mathf.Lerp(start, target, t / fadeDuration);
            yield return null;
        }

        CG.alpha = target;
        fadeRoutine = null;
    }
}