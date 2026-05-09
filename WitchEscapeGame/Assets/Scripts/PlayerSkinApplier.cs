using UnityEngine;

/// <summary>
/// Attach to the Player root. Wires up the animated PlayerModel (MainAnimation)
/// and the animated PlayerStumble / PlayerDeath (SpriteLoopAnimator).
/// Listens to SkinManager and applies the equipped skin to all three.
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
        ApplySkin(SkinManager.Instance.CurrentSkin);
    }

    void OnDestroy()
    {
        if (SkinManager.Instance != null)
            SkinManager.Instance.OnSkinEquipped -= ApplySkin;
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
