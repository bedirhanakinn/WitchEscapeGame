using System.Collections;
using UnityEngine;

/// <summary>
/// Plays a single sprite-array animation. Attach to PlayerStumble and PlayerDeath.
/// Auto-plays on OnEnable, since those GameObjects toggle on/off via PlayerController.
/// Supports looping (stumble) or play-once-and-hold-last-frame (death).
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class SpriteLoopAnimator : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public Sprite[] sprites;
    public float frameRate = 0.1f;
    public bool loop = true;

    private Coroutine running;

    void Reset() { spriteRenderer = GetComponent<SpriteRenderer>(); }

    void OnEnable() { Play(); }

    void OnDisable()
    {
        if (running != null) { StopCoroutine(running); running = null; }
    }

    public void Play()
    {
        if (running != null) StopCoroutine(running);
        if (sprites == null || sprites.Length == 0) return;
        running = StartCoroutine(PlayRoutine());
    }

    IEnumerator PlayRoutine()
    {
        do
        {
            for (int i = 0; i < sprites.Length; i++)
            {
                // Re-check each frame so live skin swaps are picked up
                if (sprites == null || i >= sprites.Length) yield break;
                spriteRenderer.sprite = sprites[i];
                yield return new WaitForSeconds(frameRate);
            }
        } while (loop);
        // Non-looping: held on last frame above.
    }

    /// <summary>Called by PlayerSkinApplier when a new skin is equipped.</summary>
    public void SetSprites(Sprite[] newSprites)
    {
        if (newSprites == null || newSprites.Length == 0) return;
        sprites = newSprites;
        if (gameObject.activeInHierarchy) Play();
    }
}
