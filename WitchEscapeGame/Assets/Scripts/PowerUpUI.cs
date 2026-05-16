using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PowerUpUI : MonoBehaviour
{
    [Header("Panel")]
    public RectTransform panel;

    public Vector2 hiddenPos;
    public Vector2 shownPos;

    [Header("Animation")]
    public float moveSpeed = 8f;
    public float visibleTime = 1.5f;

    [Header("Icons")]
    public Image[] icons;

    [Header("Opacity")]
    public float activeAlpha = 1f;
    public float inactiveAlpha = 0.5f;

    private Coroutine currentRoutine;

    void Start()
    {
        panel.anchoredPosition = hiddenPos;
    }

    public void ShowCollect(
        Sprite sprite,
        int count
    )
    {
        for (int i = 0; i < icons.Length; i++)
        {
            icons[i].sprite = sprite;

            Color c = icons[i].color;

            if (i < count)
                c.a = activeAlpha;
            else
                c.a = inactiveAlpha;

            icons[i].color = c;
        }

        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
        }

        currentRoutine =
            StartCoroutine(AnimateUI());
    }

    IEnumerator AnimateUI()
    {
        // SHOW
        while (
            Vector2.Distance(
                panel.anchoredPosition,
                shownPos
            ) > 1f
        )
        {
            panel.anchoredPosition =
                Vector2.Lerp(
                    panel.anchoredPosition,
                    shownPos,
                    moveSpeed * Time.deltaTime
                );

            yield return null;
        }

        panel.anchoredPosition = shownPos;

        yield return new WaitForSeconds(
            visibleTime
        );

        // HIDE
        while (
            Vector2.Distance(
                panel.anchoredPosition,
                hiddenPos
            ) > 1f
        )
        {
            panel.anchoredPosition =
                Vector2.Lerp(
                    panel.anchoredPosition,
                    hiddenPos,
                    moveSpeed * Time.deltaTime
                );

            yield return null;
        }

        panel.anchoredPosition = hiddenPos;
    }
}