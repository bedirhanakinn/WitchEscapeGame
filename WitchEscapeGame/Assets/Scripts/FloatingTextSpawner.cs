using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns FloatingText UI elements at a fixed HUD anchor point.
/// If multiple texts are active simultaneously, each new one is offset
/// horizontally from the previous so they don't overlap.
/// </summary>
public class FloatingTextSpawner : MonoBehaviour
{
    public static FloatingTextSpawner Instance { get; private set; }

    [SerializeField] private FloatingText floatingTextPrefab;
    [Tooltip("Where in the HUD floating text spawns.")]
    [SerializeField] private RectTransform spawnAnchor;

    [Header("Stacking")]
    [Tooltip("Horizontal offset applied to each additional active text.")]
    [SerializeField] private float stackOffsetX = 160f;

    [Tooltip("Vertical offset — staggers texts slightly so they don't look like a row.")]
    [SerializeField] private float stackOffsetY = -20f;

    [Header("Text colors per reason")]
    [SerializeField] private Color defaultColor = new Color(1f, 0.9f, 0.2f);
    [SerializeField] private Color closeShaveColor = new Color(0.6f, 0.2f, 0.8f); // purple
    [SerializeField] private Color powerUpColor = new Color(1f, 0.4f, 0.9f);

    // Track active floating texts to calculate offset
    private readonly List<RectTransform> activeTexts = new List<RectTransform>();
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

    public void Show(string text, Color color)
    {
        if (floatingTextPrefab == null || spawnAnchor == null) return;

        // Clean up any destroyed texts from the tracking list
        activeTexts.RemoveAll(rt => rt == null);

        // Calculate offset based on how many texts are currently active
        int index = activeTexts.Count;
        Vector2 offset = new Vector2(index * stackOffsetX, index * stackOffsetY);

        // Alternate left/right instead of always going right
        // 0 = center, 1 = right, 2 = left, 3 = further right, etc.
        if (index > 0)
        {
            int side = (index % 2 == 0) ? -1 : 1;
            int magnitude = (index + 1) / 2;
            offset = new Vector2(side * magnitude * stackOffsetX, magnitude * stackOffsetY);
        }

        var ft = Instantiate(floatingTextPrefab, spawnAnchor);
        ft.transform.localScale = Vector3.one;
        var rt = ft.GetComponent<RectTransform>();
        if (rt != null) rt.anchoredPosition = offset;
        ft.Configure(text, color);

        // Track this text
        if (rt != null) activeTexts.Add(rt);
    }
}