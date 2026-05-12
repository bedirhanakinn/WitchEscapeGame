using UnityEngine;

/// <summary>
/// Spawns FloatingText UI elements at a fixed HUD anchor point.
/// Listens to ScoreManager.OnBonusAwarded automatically — drop the component in
/// and bonuses appear without extra wiring.
/// </summary>
public class FloatingTextSpawner : MonoBehaviour
{
    public static FloatingTextSpawner Instance { get; private set; }

    [SerializeField] private FloatingText floatingTextPrefab;
    [Tooltip("Where in the HUD floating text spawns. Usually a RectTransform centered on screen.")]
    [SerializeField] private RectTransform spawnAnchor;

    [Header("Text colors per reason")]
    [SerializeField] private Color defaultColor = new Color(1f, 0.9f, 0.2f);   // gold
    [SerializeField] private Color closeShaveColor = new Color(0.3f, 1f, 0.4f); // green
    [SerializeField] private Color powerUpColor = new Color(1f, 0.4f, 0.9f);    // magenta

    private bool subscribed;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        TrySubscribe();
    }

    void OnDestroy()
    {
        if (subscribed && ScoreManager.Instance != null)
            ScoreManager.Instance.OnBonusAwarded -= HandleBonus;
    }

    private void TrySubscribe()
    {
        if (subscribed || ScoreManager.Instance == null) return;
        ScoreManager.Instance.OnBonusAwarded += HandleBonus;
        subscribed = true;
    }

    private void HandleBonus(int amount, string reason)
    {
        Show($"+{amount}\n<size=70%>{reason}</size>", PickColor(reason));
    }

    private Color PickColor(string reason)
    {
        if (string.IsNullOrEmpty(reason)) return defaultColor;
        string r = reason.ToLowerInvariant();
        if (r.Contains("close shave")) return closeShaveColor;
        if (r.Contains("power")) return powerUpColor;
        return defaultColor;
    }

    /// <summary>Manual show for custom calls outside the bonus flow.</summary>
    public void Show(string text, Color color)
    {
        if (floatingTextPrefab == null || spawnAnchor == null) return;
        var ft = Instantiate(floatingTextPrefab, spawnAnchor);
        ft.transform.localScale = Vector3.one;
        var rt = ft.GetComponent<RectTransform>();
        if (rt != null) rt.anchoredPosition = Vector2.zero;
        ft.Configure(text, color);
    }
}
