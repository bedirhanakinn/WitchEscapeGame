using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// One per menu panel. Handles fading the panel's CanvasGroup in/out.
/// State is driven entirely by Show/Hide/InstantShow/InstantHide — never
/// inferred from gameObject.activeSelf, which avoids race conditions
/// between UIMenu.Awake and UIManager.Awake.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public class UIMenu : MonoBehaviour
{
    [Header("Identity")]
    [Tooltip("Unique ID for this menu. Must match an entry in UIManager.MenuId.")]
    public UIManager.MenuId menuId;

    [Header("Fade")]
    [Tooltip("How long the fade in/out takes, in seconds (unscaled time).")]
    public float fadeDuration = 0.25f;

    [Header("Events")]
    [Tooltip("Fired when this menu starts fading in. Use to show accompanying UI (e.g. Background).")]
    public UnityEvent onShow;
    [Tooltip("Fired when this menu starts fading out. Use to hide accompanying UI (e.g. Background).")]
    public UnityEvent onHide;

    private CanvasGroup canvasGroup;
    private Coroutine fadeRoutine;

    public bool IsOpen { get; private set; }

    /// <summary>Lazy CanvasGroup accessor — works even if Awake hasn't run yet
    /// (which happens when this GameObject starts inactive in the scene).</summary>
    private CanvasGroup CG
    {
        get
        {
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
            return canvasGroup;
        }
    }

    /// <summary>Fade this menu in. Safe to call from Button.onClick directly.</summary>
    public void Show()
    {
        if (IsOpen) return;
        IsOpen = true;

        gameObject.SetActive(true);
        onShow?.Invoke();
        StartFade(1f);
    }

    /// <summary>Fade this menu out. Safe to call from Button.onClick directly.</summary>
    public void Hide()
    {
        if (!IsOpen) return;
        IsOpen = false;

        onHide?.Invoke();
        StartFade(0f);
    }

    /// <summary>Show with no animation. Used by UIManager during initialization.</summary>
    public void InstantShow()
    {
        StopFade();
        gameObject.SetActive(true);
        CG.alpha = 1f;
        CG.interactable = true;
        CG.blocksRaycasts = true;
        IsOpen = true;
        // Note: onShow is intentionally NOT fired here — Instant variants are
        // used for initialization only, not player-driven transitions.
    }

    /// <summary>Hide with no animation. Used by UIManager during initialization.</summary>
    public void InstantHide()
    {
        StopFade();
        CG.alpha = 0f;
        CG.interactable = false;
        CG.blocksRaycasts = false;
        gameObject.SetActive(false);
        IsOpen = false;
        // Note: onHide is intentionally NOT fired here — same reason as above.
    }

    /// <summary>
    /// Inspector helper — wire this directly to a Button.onClick to open this
    /// menu through the UIManager (so the menu stack is updated correctly).
    /// </summary>
    public void OpenViaManager()
    {
        if (UIManager.Instance != null)
            UIManager.Instance.Open(menuId);
    }

    void StopFade()
    {
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
            fadeRoutine = null;
        }
    }

    void StartFade(float targetAlpha)
    {
        StopFade();
        fadeRoutine = StartCoroutine(FadeRoutine(targetAlpha));
    }

    IEnumerator FadeRoutine(float target)
    {
        bool becomingInteractive = target > 0f;
        CG.interactable = becomingInteractive;
        CG.blocksRaycasts = becomingInteractive;

        float start = CG.alpha;
        float t = 0f;

        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            CG.alpha = Mathf.Lerp(start, target, t / fadeDuration);
            yield return null;
        }

        CG.alpha = target;

        if (Mathf.Approximately(target, 0f))
            gameObject.SetActive(false);

        fadeRoutine = null;
    }
}