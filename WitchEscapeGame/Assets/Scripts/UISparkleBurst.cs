using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// A purple sparkle burst that renders natively inside a uGUI Canvas (works with
/// Screen Space - Overlay, unlike a ParticleSystem). Spawns a ring of small
/// Image sparkles that fly outward, spin, and fade out, then clean themselves up.
///
/// Put this on a child of the SkinCard (e.g. an empty "UnlockParticles" RectTransform
/// centered on the card). Call Burst() to trigger it.
/// </summary>
public class UISparkleBurst : MonoBehaviour
{
    [Header("Sparkle Look")]
    [Tooltip("Sprite used for each sparkle. A small soft circle, star, or dot works best. " +
             "If left empty, a plain square is used.")]
    [SerializeField] private Sprite sparkleSprite;

    [Tooltip("Purple tint for the sparkles.")]
    [SerializeField] private Color sparkleColor = new Color(0.55f, 0.36f, 0.75f, 1f);

    [Tooltip("How many sparkles per burst.")]
    [SerializeField] private int sparkleCount = 16;

    [Tooltip("Size of each sparkle in UI units.")]
    [SerializeField] private float sparkleSize = 28f;

    [Header("Motion")]
    [Tooltip("How far sparkles travel from the center.")]
    [SerializeField] private float travelDistance = 180f;

    [Tooltip("Seconds for the full burst to play and fade.")]
    [SerializeField] private float duration = 0.7f;

    [Tooltip("Random size variation (0 = none, 0.5 = +/-50%).")]
    [SerializeField] private float sizeJitter = 0.4f;

    private readonly List<RectTransform> _pool = new List<RectTransform>();

    /// <summary>
    /// Triggers the sparkle burst. Safe to call repeatedly.
    /// </summary>
    public void Burst()
    {
        StopAllCoroutines();
        StartCoroutine(BurstRoutine());
    }

    private IEnumerator BurstRoutine()
    {
        // Build sparkles if we don't have enough pooled.
        while (_pool.Count < sparkleCount)
            _pool.Add(CreateSparkle());

        // Set up each sparkle's direction + size for this burst.
        var dirs = new Vector2[sparkleCount];
        var sizes = new float[sparkleCount];
        for (int i = 0; i < sparkleCount; i++)
        {
            float ang = (i / (float)sparkleCount) * Mathf.PI * 2f + Random.Range(-0.15f, 0.15f);
            dirs[i] = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang));
            sizes[i] = sparkleSize * (1f + Random.Range(-sizeJitter, sizeJitter));

            var rt = _pool[i];
            rt.gameObject.SetActive(true);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(sizes[i], sizes[i]);
            var img = rt.GetComponent<Image>();
            img.color = sparkleColor;
        }

        // Animate using unscaled time so it plays even if the game is paused.
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float eased = 1f - (1f - t) * (1f - t); // ease-out

            for (int i = 0; i < sparkleCount; i++)
            {
                var rt = _pool[i];
                rt.anchoredPosition = dirs[i] * (travelDistance * eased);
                rt.localRotation = Quaternion.Euler(0, 0, t * 180f);

                var img = rt.GetComponent<Image>();
                var c = img.color;
                c.a = 1f - t; // fade out
                img.color = c;

                float s = sizes[i] * (1f - 0.4f * t); // slight shrink
                rt.sizeDelta = new Vector2(s, s);
            }
            yield return null;
        }

        // Hide all sparkles when done.
        for (int i = 0; i < _pool.Count; i++)
            _pool[i].gameObject.SetActive(false);
    }

    private RectTransform CreateSparkle()
    {
        var go = new GameObject("Sparkle", typeof(RectTransform), typeof(Image));
        var rt = go.GetComponent<RectTransform>();
        rt.SetParent(transform, false);
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;

        var img = go.GetComponent<Image>();
        if (sparkleSprite != null) img.sprite = sparkleSprite;
        img.color = sparkleColor;
        img.raycastTarget = false; // never block clicks

        go.SetActive(false);
        return rt;
    }
}
