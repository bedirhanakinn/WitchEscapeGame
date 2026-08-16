using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// On the card prefab. Self-contained Buy/Equip/Equipped/Locked logic.
/// ShopController pushes lock state in via SetLocked() before each Refresh().
/// </summary>
public class SkinCard : MonoBehaviour
{
    public enum LockedSpriteMode { Hide, GreyOut, Off }

    // ---- FLAG: field names below (previewImage/nameText) are reconstructed. ----
    [Header("Data Display")]
    [SerializeField] private Image previewImage;
    [SerializeField] private TextMeshProUGUI nameText;

    [Header("Button")]
    [SerializeField] private Button actionButton;
    [SerializeField] private TextMeshProUGUI buttonLabel;
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private Image buttonBackground;

    [Header("Button State Sprites")]
    [SerializeField] private Sprite buySprite;
    [SerializeField] private Sprite cantAffordSprite;
    [SerializeField] private Sprite equipSprite;
    [SerializeField] private Sprite equippedSprite;

    [Header("Purchase Feedback")]
    [SerializeField] private AudioClip unlockSound;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private ParticleSystem unlockParticles;

    [Header("Lock State")]
    [SerializeField] private GameObject lockOverlay;        // holds lock icon + message, covers the card
    [SerializeField] private TextMeshProUGUI lockMessage;   // "Buy X more to unlock"
    [SerializeField] private string lockedFormat = "Buy {0} more to unlock";

    [Header("Lock Visual")]
    [SerializeField] private LockedSpriteMode lockedSpriteMode = LockedSpriteMode.GreyOut;
    [SerializeField] private Color lockedTint = new Color(0.15f, 0.15f, 0.15f, 1f); // dark silhouette
    private Color _previewDefaultColor = Color.white;

    private SkinData skin;
    private bool _wasOwned;
    private bool _locked;
    private int _lockNeeded;

    public void Initialize(SkinData data)
    {
        skin = data;

        if (previewImage != null)
        {
            previewImage.sprite = data.GetPreview();
            _previewDefaultColor = previewImage.color;   // capture default for restore
        }
        if (nameText != null) nameText.text = data.displayName;

        // ---- FLAG: if your button is ALREADY wired to OnActionClicked in the
        // Inspector, DELETE these two lines or every buy/equip will fire twice. ----
        if (actionButton != null)
        {
            actionButton.onClick.RemoveListener(OnActionClicked);
            actionButton.onClick.AddListener(OnActionClicked);
        }

        if (SkinManager.Instance != null)
            _wasOwned = SkinManager.Instance.IsOwned(skin.skinId);

        Refresh();
    }

    void OnEnable()
    {
        if (skin == null) return;   // guard: OnEnable can fire before Initialize

        if (SkinManager.Instance != null)
            _wasOwned = SkinManager.Instance.IsOwned(skin.skinId);

        if (unlockSound != null && audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }

        Refresh();
    }

    /// <summary>Called by ShopController before Refresh(). Stores the flag + message count.</summary>
    public void SetLocked(bool locked, int needed)
    {
        _locked = locked;
        _lockNeeded = needed;
    }

    /// <summary>Called by ShopController when currency, equip, or lock state changes.</summary>
    public void Refresh()
    {
        if (skin == null) return;

        if (SkinManager.Instance == null || CurrencyManager.Instance == null)
        {
            Debug.LogWarning("SkinCard.Refresh: SkinManager or CurrencyManager not in scene yet. " +
                             "Make sure both GameObjects exist and SkinManager has a Database assigned.");
            return;
        }

        // Locked tier: viewable but greyed, no interaction, dynamic message.
        if (_locked)
        {
            if (lockOverlay != null) lockOverlay.SetActive(true);
            if (lockMessage != null) lockMessage.text = string.Format(lockedFormat, _lockNeeded);

            if (previewImage != null)
            {
                switch (lockedSpriteMode)
                {
                    case LockedSpriteMode.Hide:
                        previewImage.enabled = false;
                        break;
                    case LockedSpriteMode.GreyOut:
                        previewImage.enabled = true;
                        previewImage.color = lockedTint;
                        break;
                    case LockedSpriteMode.Off:
                        previewImage.enabled = true;
                        break;
                }
            }

            buttonLabel.text = "LOCKED";
            if (priceText != null) priceText.text = "";
            actionButton.interactable = false;
            // Keep _wasOwned honest so unlocking later doesn't misfire the purchase FX.
            _wasOwned = SkinManager.Instance.IsOwned(skin.skinId);
            return;
        }

        // Unlocked: restore overlay + sprite to normal.
        if (lockOverlay != null) lockOverlay.SetActive(false);
        if (previewImage != null)
        {
            previewImage.enabled = true;
            previewImage.color = _previewDefaultColor;
        }

        bool owned = SkinManager.Instance.IsOwned(skin.skinId);
        bool equipped = SkinManager.Instance.IsEquipped(skin);
        bool canAfford = CurrencyManager.Instance.CanAfford(skin.price);

        // Detect a fresh purchase: was not owned, now is owned.
        if (!_wasOwned && owned)
        {
            PlayUnlockFeedback();
        }
        _wasOwned = owned;

        if (equipped)
        {
            buttonLabel.text = "EQUIPPED";
            if (priceText != null) priceText.text = "";
            actionButton.interactable = false;
            SetButtonSprite(equippedSprite);
        }
        else if (owned)
        {
            buttonLabel.text = "EQUIP";
            if (priceText != null) priceText.text = "";
            actionButton.interactable = true;
            SetButtonSprite(equipSprite);
        }
        else
        {
            buttonLabel.text = "BUY";
            if (priceText != null) priceText.text = skin.price.ToString();
            actionButton.interactable = canAfford;
            SetButtonSprite(canAfford ? buySprite : cantAffordSprite);
        }
    }

    private void SetButtonSprite(Sprite s)
    {
        if (buttonBackground != null && s != null)
            buttonBackground.sprite = s;
    }

    private void PlayUnlockFeedback()
    {
        if (unlockSound != null && audioSource != null)
            audioSource.PlayOneShot(unlockSound);

        if (unlockParticles != null)
        {
            unlockParticles.Clear();
            unlockParticles.Play();
        }
    }

    private void OnActionClicked()
    {
        if (skin == null) return;

        if (SkinManager.Instance.IsOwned(skin.skinId))
            SkinManager.Instance.Equip(skin);
        else
            SkinManager.Instance.TryBuy(skin);
    }
}