using TMPro;
using UnityEngine;

/// <summary>
/// On a screen-space UI prefab. Shows a label, animates it (scale-pop + fade + drift),
/// then destroys itself.
/// </summary>
public class FloatingText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform rectTransform;

    [Header("Animation")]
    [SerializeField] private float lifetime = 1.2f;
    [SerializeField] private float driftDistance = 60f; // pixels up
    [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0.6f, 1f, 1f);
    [SerializeField] private AnimationCurve alphaCurve = AnimationCurve.Linear(0, 1, 1, 0);
    [SerializeField] private AnimationCurve driftCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Vector2 startPos;
    private float age;

    void Awake()
    {
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
    }

    void Start()
    {
        startPos = rectTransform.anchoredPosition;
    }

    public void Configure(string text, Color color)
    {
        if (label != null)
        {
            label.text = text;
            label.color = color;
        }
    }

    void Update()
    {
        // Uses unscaled time so it animates even if Time.timeScale = 0 (e.g. paused)
        age += Time.unscaledDeltaTime;
        float t = Mathf.Clamp01(age / lifetime);

        if (rectTransform != null)
        {
            float drift = driftCurve.Evaluate(t) * driftDistance;
            rectTransform.anchoredPosition = startPos + new Vector2(0, drift);
            float s = scaleCurve.Evaluate(t);
            rectTransform.localScale = new Vector3(s, s, 1);
        }

        if (canvasGroup != null)
            canvasGroup.alpha = alphaCurve.Evaluate(t);

        if (t >= 1f) Destroy(gameObject);
    }
}
