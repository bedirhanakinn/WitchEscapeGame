using UnityEngine;

[CreateAssetMenu(fileName = "NewSkin", menuName = "WitchEscape/Skin Data")]
public class SkinData : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("Unique ID used for save data. NEVER change after release.")]
    public string skinId;
    public string displayName;

    [Header("PlayerModel (animated via MainAnimation)")]
    public Sprite[] upSprites;
    public Sprite[] downSprites;
    public Sprite[] throwSprites;

    [Header("PlayerStumble (animated via SpriteLoopAnimator, loops)")]
    public Sprite[] stumbleSprites;

    [Header("PlayerDeath (animated via SpriteLoopAnimator, plays once)")]
    public Sprite[] deathSprites;

    [Header("Main Menu states (used before run starts)")]
    [Tooltip("Animated burning intro shown when scene loads.")]
    public Sprite[] burningSprites;

    [Tooltip("Single sprite shown after tap, before run starts.")]
    public Sprite whistlingSprite;

    [Tooltip("Single sprite shown briefly between whistling and game start.")]
    public Sprite lookingUpSprite;

    [Header("Shop Display")]
    [Tooltip("Optional. If empty, falls back to first frame of upSprites.")]
    public Sprite shopPreview;
    public int price;

    [Tooltip("Tick on the starter skin so it's owned for free on first launch.")]
    public bool ownedByDefault;

    public Sprite GetPreview()
    {
        if (shopPreview != null) return shopPreview;
        if (upSprites != null && upSprites.Length > 0) return upSprites[0];
        return null;
    }
}