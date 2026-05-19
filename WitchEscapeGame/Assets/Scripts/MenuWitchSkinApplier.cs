using UnityEngine;

/// <summary>
/// Sits in the gameplay scene. References the 3 menu witch GameObjects (Burning,
/// Whistling, Looking Up). On scene load, reads the currently-equipped skin from
/// SkinManager and applies the matching menu sprites.
///
/// Strict mode: warns if any menu sprite is missing on the equipped skin.
/// You can disable warnings via the warnOnMissing toggle once art is complete.
/// </summary>
public class MenuWitchSkinApplier : MonoBehaviour
{
    [Header("Burning (animated)")]
    [Tooltip("SpriteLoopAnimator on the Burning Witch GameObject.")]
    [SerializeField] private SpriteLoopAnimator burningAnimator;

    [Header("Whistling (static)")]
    [Tooltip("SpriteRenderer on the Whistling Witch GameObject.")]
    [SerializeField] private SpriteRenderer whistlingRenderer;

    [Header("Looking Up (static)")]
    [Tooltip("SpriteRenderer on the Looking Up Witch GameObject.")]
    [SerializeField] private SpriteRenderer lookingUpRenderer;

    [Header("Debug")]
    [Tooltip("Log a warning if the equipped skin is missing any menu sprites. Turn off when art is complete.")]
    [SerializeField] private bool warnOnMissing = true;

    void Start()
    {
        if (SkinManager.Instance == null)
        {
            Debug.LogError("MenuWitchSkinApplier: SkinManager not found in scene.");
            return;
        }

        ApplySkin(SkinManager.Instance.CurrentSkin);
    }

    private void ApplySkin(SkinData skin)
    {
        if (skin == null)
        {
            if (warnOnMissing)
                Debug.LogWarning("MenuWitchSkinApplier: No skin equipped, menu witches will show their default sprites.");
            return;
        }

        // Burning
        if (burningAnimator != null)
        {
            if (skin.burningSprites != null && skin.burningSprites.Length > 0)
                burningAnimator.SetSprites(skin.burningSprites);
            else if (warnOnMissing)
                Debug.LogWarning($"MenuWitchSkinApplier: Skin '{skin.skinId}' has no burningSprites assigned.");
        }

        // Whistling
        if (whistlingRenderer != null)
        {
            if (skin.whistlingSprite != null)
                whistlingRenderer.sprite = skin.whistlingSprite;
            else if (warnOnMissing)
                Debug.LogWarning($"MenuWitchSkinApplier: Skin '{skin.skinId}' has no whistlingSprite assigned.");
        }

        // Looking Up
        if (lookingUpRenderer != null)
        {
            if (skin.lookingUpSprite != null)
                lookingUpRenderer.sprite = skin.lookingUpSprite;
            else if (warnOnMissing)
                Debug.LogWarning($"MenuWitchSkinApplier: Skin '{skin.skinId}' has no lookingUpSprite assigned.");
        }
    }
}
