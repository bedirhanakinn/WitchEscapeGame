using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PowerUpUI : MonoBehaviour
{
    [Header("Panel")]
    public RectTransform panel;
    public Vector2 hiddenPos;
    public Vector2 shownPos;

    [Header("Animation Speeds")]
    public float moveSpeed = 15f;
    public float visibleTime = 1.2f;
    public float xFadeDuration = 0.12f;

    [Header("UI Elements")]
    public Image[] icons;
    public Image[] cancelXOverlays;

    [Header("Opacity Settings")]
    public float activeAlpha = 1f;
    public float inactiveAlpha = 0.5f;

    private Coroutine currentRoutine;
    
    // Persistent tracking of what streak the player is currently building
    private CollectibleType currentType;
    private int currentCount = 0;
    private bool isPanelShown = false;

    void Start()
    {
        panel.anchoredPosition = hiddenPos;
        SetXAlpha(0f, 0); // Hide all Xs on start
    }

    public void ShowCollect(Sprite sprite, int count, CollectibleType type)
    {
        // STREAK BROKEN IF:
        // Player had at least 1 item saved (currentCount > 0), AND the new item is a DIFFERENT type.
        bool streakBroken = (currentCount > 0 && currentType != type);

        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
        }

        // Always force X alpha back to 0 on new collection to avoid visual stuck artifacts
        SetXAlpha(0f, 0);

        currentRoutine = StartCoroutine(HandleCollectSequence(sprite, count, type, streakBroken));
    }

    IEnumerator HandleCollectSequence(Sprite newSprite, int newCount, CollectibleType newType, bool streakBroken)
    {
        // Cache the previous count BEFORE we update state, so we know how many Xs to draw
        int boxesToCancel = currentCount;

        // STEP 1: Bring down panel if it's currently hidden
        if (!isPanelShown)
        {
            if (!streakBroken)
            {
                UpdateIcons(newSprite, newCount);
            }
            
            yield return StartCoroutine(MovePanel(shownPos));
            isPanelShown = true;
        }

        // STEP 2: Handle streak break (Flash X ONLY over active boxes)
        if (streakBroken)
        {
            // Fade IN X only over the active boxes (e.g., 1 X for 1 frog, 2 Xs for 2 frogs)
            yield return StartCoroutine(FadeXOverlays(0f, 1f, xFadeDuration, boxesToCancel));
            yield return new WaitForSeconds(0.08f);
            
            // Swap icons to the new pickup type while X covers them
            UpdateIcons(newSprite, newCount);
            
            // Fade OUT X
            yield return StartCoroutine(FadeXOverlays(1f, 0f, xFadeDuration, boxesToCancel));
        }
        else
        {
            // No streak break: update icons instantly
            UpdateIcons(newSprite, newCount);
        }

        // UPDATE SAVED STREAK STATE
        currentType = newType;
        currentCount = newCount;

        // If player hit 3 of a kind, streak completes! Reset counter so next pickup starts fresh
        if (currentCount >= 3)
        {
            currentCount = 0;
        }

        // STEP 3: Hold panel visible on screen
        yield return new WaitForSeconds(visibleTime);

        // STEP 4: Hide panel back up
        yield return StartCoroutine(MovePanel(hiddenPos));
        isPanelShown = false;
    }

    void UpdateIcons(Sprite sprite, int count)
    {
        for (int i = 0; i < icons.Length; i++)
        {
            if (sprite != null)
            {
                icons[i].sprite = sprite;
                icons[i].enabled = true;

                Color c = icons[i].color;
                c.a = (i < count) ? activeAlpha : inactiveAlpha;
                icons[i].color = c;
            }
            else
            {
                icons[i].enabled = false;
            }
        }
    }

    IEnumerator MovePanel(Vector2 targetPos)
    {
        while (Vector2.Distance(panel.anchoredPosition, targetPos) > 0.5f)
        {
            panel.anchoredPosition = Vector2.MoveTowards(
                panel.anchoredPosition,
                targetPos,
                moveSpeed * Time.deltaTime * 500f
            );
            yield return null;
        }
        panel.anchoredPosition = targetPos;
    }

    IEnumerator FadeXOverlays(float startAlpha, float targetAlpha, float duration, int activeBoxCount)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float currentAlpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
            SetXAlpha(currentAlpha, activeBoxCount);
            yield return null;
        }
        SetXAlpha(targetAlpha, activeBoxCount);
    }

    void SetXAlpha(float alpha, int activeBoxCount)
    {
        for (int i = 0; i < cancelXOverlays.Length; i++)
        {
            if (cancelXOverlays[i] != null)
            {
                Color c = cancelXOverlays[i].color;
                
                // Only show the X if this box index was active before the break
                if (i < activeBoxCount)
                {
                    c.a = alpha;
                }
                else
                {
                    c.a = 0f; // Keep empty boxes hidden
                }

                cancelXOverlays[i].color = c;
            }
        }
    }
}