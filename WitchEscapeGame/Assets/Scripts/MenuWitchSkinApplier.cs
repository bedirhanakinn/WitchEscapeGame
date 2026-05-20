using UnityEngine;

/// <summary>
/// Applies the equipped skin's menu sprites to the three menu witch GameObjects.
/// Reacts to live skin changes via SkinManager.OnSkinEquipped so equipping a
/// skin in the shop immediately updates the menu witches without a scene reload.
/// </summary>
public class MenuWitchSkinApplier : MonoBehaviour
{
    [Header("Burning (animated)")]
    [SerializeField] private SpriteLoopAnimator burningAnimator;

    [Header("Whistling (static)")]
    [SerializeField] private SpriteRenderer whistlingRenderer;

    [Header("Looking Up (static)")]
    [SerializeField] private SpriteRenderer lookingUpRenderer;

    [Header("Debug")]
    [Tooltip("Log a warning if the equipped skin is missing any menu sprites.")]
    [SerializeField] private bool warnOnMissing = true;

    void Start()
    {
        if (SkinManager.Instance == null)
        {
            Debug.LogError("MenuWitchSkinApplier: SkinManager not found.");
            return;
        }

        // Apply current skin immediately
        ApplySkin(SkinManager.Instance.CurrentSkin);

        // Subscribe so future equips update the menu witches live
        SkinManager.Instance.OnSkinEquipped += ApplySkin;
    }

    void OnDestroy()
    {
        if (SkinManager.Instance != null)
            SkinManager.Instance.OnSkinEquipped -= ApplySkin;
    }

    private void ApplySkin(SkinData skin)
    {
        if (skin == null)
        {
            if (warnOnMissing)
                Debug.LogWarning("MenuWitchSkinApplier: No skin equipped.");
            return;
        }

        // Burning
        if (burningAnimator != null)
        {
            if (skin.burningSprites != null && skin.burningSprites.Length > 0)
                burningAnimator.SetSprites(skin.burningSprites);
            else if (warnOnMissing)
                Debug.LogWarning($"MenuWitchSkinApplier: '{skin.skinId}' missing burningSprites.");
        }

        // Whistling
        if (whistlingRenderer != null)
        {
            if (skin.whistlingSprite != null)
                whistlingRenderer.sprite = skin.whistlingSprite;
            else if (warnOnMissing)
                Debug.LogWarning($"MenuWitchSkinApplier: '{skin.skinId}' missing whistlingSprite.");
        }

        // Looking Up
        if (lookingUpRenderer != null)
        {
            if (skin.lookingUpSprite != null)
                lookingUpRenderer.sprite = skin.lookingUpSprite;
            else if (warnOnMissing)
                Debug.LogWarning($"MenuWitchSkinApplier: '{skin.skinId}' missing lookingUpSprite.");
        }
    }
}