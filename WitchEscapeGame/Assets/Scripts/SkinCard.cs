using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// One of these per card prefab. Shows the skin preview, price, and a
/// context-aware action button (Buy / Can't Afford / Equip / Equipped).
///
/// Button states are shown by swapping the button background SPRITE.
/// On a successful PURCHASE (not equip), plays an unlock sound and a
/// purple particle burst around the card.
/// </summary>
public class SkinCard : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Image previewImage;
    [SerializeField] private TextMeshProUGUI displayNameText; // optional
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private TextMeshProUGUI buttonLabel;
    [SerializeField] private Button actionButton;
    [SerializeField] private Image buttonBackground;

    [Header("Button State Sprites")]
    [Tooltip("Green – owned=false, can afford. The 'purchase' state.")]
    [SerializeField] private Sprite buySprite;

    [Tooltip("Grey – owned=false, cannot afford.")]
    [SerializeField] private Sprite cantAffordSprite;

    [Tooltip("Blue – owned, not equipped.")]
    [SerializeField] private Sprite equipSprite;

    [Tooltip("Gold – owned and equipped.")]
    [SerializeField] private Sprite equippedSprite;

    [Header("Purchase Feedback")]
    [Tooltip("Sound played once when this skin is successfully purchased.")]
    [SerializeField] private AudioClip unlockSound;

    [SerializeField] private float unlockSoundVolume = 1f;

    [Tooltip("UI sparkle burst played on purchase.")]
    [SerializeField] private UISparkleBurst unlockParticles;

    private SkinData skin;

    // Tracks ownership across refreshes so we can detect the
    // not-owned -> owned transition.
    private bool _wasOwned;

    public void Initialize(SkinData data)
    {
        skin = data;

        if (previewImage != null)
            previewImage.sprite = data.GetPreview();

        if (displayNameText != null)
            displayNameText.text = data.displayName;

        actionButton.onClick.RemoveAllListeners();
        actionButton.onClick.AddListener(OnActionClicked);

        // Establish the starting ownership baseline.
        if (SkinManager.Instance != null)
            _wasOwned = SkinManager.Instance.IsOwned(skin.skinId);

        Refresh();
    }

    /// <summary>
    /// Called by ShopController when currency or equipped skin changes.
    /// </summary>
    public void Refresh()
    {
        if (skin == null) return;

        if (SkinManager.Instance == null || CurrencyManager.Instance == null)
        {
            Debug.LogWarning("SkinCard.Refresh: SkinManager or CurrencyManager not in scene yet. Make sure both GameObjects exist and SkinManager has a Database assigned.");
            return;
        }

        bool owned = SkinManager.Instance.IsOwned(skin.skinId);
        bool equipped = SkinManager.Instance.IsEquipped(skin);
        bool canAfford = CurrencyManager.Instance.CanAfford(skin.price);

        // Detect a fresh purchase.
        if (!_wasOwned && owned)
        {
            PlayUnlockFeedback();
        }

        _wasOwned = owned;

        if (equipped)
        {
            buttonLabel.text = "EQUIPPED";

            if (priceText != null)
                priceText.text = "";

            actionButton.interactable = false;
            SetButtonSprite(equippedSprite);
        }
        else if (owned)
        {
            buttonLabel.text = "EQUIP";

            if (priceText != null)
                priceText.text = "";

            actionButton.interactable = true;
            SetButtonSprite(equipSprite);
        }
        else
        {
            buttonLabel.text = "BUY";

            if (priceText != null)
                priceText.text = skin.price.ToString();

            actionButton.interactable = canAfford;
            SetButtonSprite(canAfford ? buySprite : cantAffordSprite);
        }
    }

    private void SetButtonSprite(Sprite s)
    {
        if (buttonBackground != null && s != null)
            buttonBackground.sprite = s;
    }

    /// <summary>
    /// Plays the unlock sound and particle burst.
    /// Called only after a successful purchase.
    /// </summary>
    private void PlayUnlockFeedback()
    {
        if (unlockSound != null)
        {
            SoundManager.Instance.PlaySFX(unlockSound, unlockSoundVolume);
        }

        if (unlockParticles != null)
        {
            unlockParticles.Burst();
        }
    }

    private void OnActionClicked()
    {
        if (skin == null)
            return;

        if (SkinManager.Instance.IsOwned(skin.skinId))
            SkinManager.Instance.Equip(skin);
        else
            SkinManager.Instance.TryBuy(skin);

        // ShopController listens for events and refreshes every card,
        // which triggers purchase detection in Refresh().
    }
}