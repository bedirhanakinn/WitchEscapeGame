using UnityEngine;
using System.Collections;

public class MainAnimation : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;

    [Header("Animations")]
    public Sprite[] upSprites;
    public Sprite[] downSprites;
    public Sprite[] throwSprites;

    public float frameRate = 0.1f;

    private Coroutine currentAnimation;
    private string currentAnimationName = "";
    private bool currentLoop;

    public void PlayUp()    { StartAnimation("Up", true); }
    public void PlayDown()  { StartAnimation("Down", true); }
    public void PlayThrow() { StartAnimation("Throw", false); }

    void StartAnimation(string animationName, bool loop)
    {
        if (currentAnimationName == animationName) return;

        if (currentAnimation != null) StopCoroutine(currentAnimation);

        currentAnimationName = animationName;
        currentLoop = loop;
        currentAnimation = StartCoroutine(PlaySprites(animationName, loop));
    }

    IEnumerator PlaySprites(string animationName, bool loop)
    {
        do
        {
            Sprite[] sprites = GetSpritesFor(animationName);
            if (sprites == null || sprites.Length == 0) yield break;

            for (int i = 0; i < sprites.Length; i++)
            {
                // Re-fetch each frame so live skin swaps are picked up immediately
                Sprite[] current = GetSpritesFor(animationName);
                if (current == null || i >= current.Length) break;
                spriteRenderer.sprite = current[i];
                yield return new WaitForSeconds(frameRate);
            }
        } while (loop);
    }

    Sprite[] GetSpritesFor(string name)
    {
        switch (name)
        {
            case "Up":    return upSprites;
            case "Down":  return downSprites;
            case "Throw": return throwSprites;
            default:      return null;
        }
    }

    /// <summary>
    /// Called by PlayerSkinApplier. Swaps the sprite arrays and (if mid-animation)
    /// the running coroutine picks up the new frames on the next iteration.
    /// </summary>
    public void SetSkinSprites(Sprite[] up, Sprite[] down, Sprite[] throw_)
    {
        if (up != null && up.Length > 0)         upSprites = up;
        if (down != null && down.Length > 0)     downSprites = down;
        if (throw_ != null && throw_.Length > 0) throwSprites = throw_;

        // If currently showing a sprite, refresh it immediately to avoid
        // a stale frame until the next frameRate tick.
        if (!string.IsNullOrEmpty(currentAnimationName))
        {
            Sprite[] s = GetSpritesFor(currentAnimationName);
            if (s != null && s.Length > 0) spriteRenderer.sprite = s[0];
        }
    }

    void OnDisable()
    {
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
            currentAnimation = null;
            currentAnimationName = "";
        }
    }
}
