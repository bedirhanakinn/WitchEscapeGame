using System.Collections;
using UnityEngine;

/// <summary>
/// Attach to the Player root. Wires up the animated PlayerModel (MainAnimation)
/// and the animated PlayerStumble / PlayerDeath (SpriteLoopAnimator).
/// Listens to SkinManager and applies the equipped skin to all three.
///
/// Uses a one-frame delay on initial apply to guarantee it runs AFTER all other
/// Start() methods (including any that might reset SpriteLoopAnimator sprites
/// from Inspector defaults).
/// </summary>
public class PlayerSkinApplier : MonoBehaviour
{
    [Header("PlayerModel (Up / Down / Throw)")]
    [SerializeField] private MainAnimation playerModelAnimation;

    [Header("PlayerStumble (looping)")]
    [SerializeField] private SpriteLoopAnimator stumbleAnimator;

    [Header("PlayerDeath (one-shot)")]
    [SerializeField] private SpriteLoopAnimator deathAnimator;

    void Start()
    {
        if (SkinManager.Instance == null)
        {
            Debug.LogError("PlayerSkinApplier: SkinManager not found in scene.");
            return;
        }

        SkinManager.Instance.OnSkinEquipped += ApplySkin;

        // Delay by one frame so all other Start() methods finish first.
        // This prevents Inspector-default sprites from overwriting the skin
        // after ApplySkin runs.
        StartCoroutine(ApplyNextFrame());
    }

    void OnDestroy()
    {
        if (SkinManager.Instance != null)
            SkinManager.Instance.OnSkinEquipped -= ApplySkin;
    }

    IEnumerator ApplyNextFrame()
    {
        yield return null; // wait one frame
        ApplySkin(SkinManager.Instance != null ? SkinManager.Instance.CurrentSkin : null);
    }

    private void ApplySkin(SkinData skin)
    {
        if (skin == null) return;

        if (playerModelAnimation != null)
            playerModelAnimation.SetSkinSprites(skin.upSprites, skin.downSprites, skin.throwSprites);

        if (stumbleAnimator != null)
            stumbleAnimator.SetSprites(skin.stumbleSprites);

        if (deathAnimator != null)
            deathAnimator.SetSprites(skin.deathSprites);
    }
}