using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// One of these per card prefab. Shows the skin preview, price, and a
/// context-aware action button (Buy / Can't Afford / Equip / Equipped).
///
/// Button states are shown by swapping the button background SPRITE (not by
/// color-tinting a single sprite), so each state has its own clean scalloped art.
/// </summary>
public class SkinCard : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Image previewImage;
    [SerializeField] private TextMeshProUGUI displayNameText; // optional
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private TextMeshProUGUI buttonLabel;
    [SerializeField] private Button actionButton;
    [SerializeField] private Image buttonBackground; // the button's background Image (sprite gets swapped)

    [Header("Button State Sprites")]
    [Tooltip("Green — owned=false, can afford. The 'purchase' state.")]
    [SerializeField] private Sprite buySprite;
    [Tooltip("Grey — owned=false, cannot afford. Disabled-looking.")]
    [SerializeField] private Sprite cantAffordSprite;
    [Tooltip("Blue — owned, not equipped. Click to equip.")]
    [SerializeField] private Sprite equipSprite;
    [Tooltip("Gold — owned and equipped. The active state.")]
    [SerializeField] private Sprite equippedSprite;

    private SkinData skin;

    public void Initialize(SkinData data)
    {
        skin = data;

        if (previewImage != null)
            previewImage.sprite = data.GetPreview();

        if (displayNameText != null)
            displayNameText.text = data.displayName;

        actionButton.onClick.RemoveAllListeners();
        actionButton.onClick.AddListener(OnActionClicked);

        Refresh();
    }

    /// <summary>Called by ShopController when currency or equipped skin changes.</summary>
    public void Refresh()
    {
        if (skin == null) return;

        if (SkinManager.Instance == null || CurrencyManager.Instance == null)
        {
            Debug.LogWarning("SkinCard.Refresh: SkinManager or CurrencyManager not in scene yet. " +
                             "Make sure both GameObjects exist and SkinManager has a Database assigned.");
            return;
        }

        bool owned = SkinManager.Instance.IsOwned(skin.skinId);
        bool equipped = SkinManager.Instance.IsEquipped(skin);
        bool canAfford = CurrencyManager.Instance.CanAfford(skin.price);

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

    /// <summary>
    /// Swaps the button background to the sprite for the current state.
    /// Keeps Image Type as-is (set it to Sliced in the Inspector for 9-slice).
    /// </summary>
    private void SetButtonSprite(Sprite s)
    {
        if (buttonBackground != null && s != null)
            buttonBackground.sprite = s;
    }

    private void OnActionClicked()
    {
        if (skin == null) return;

        if (SkinManager.Instance.IsOwned(skin.skinId))
            SkinManager.Instance.Equip(skin);
        else
            SkinManager.Instance.TryBuy(skin);

        // ShopController hears the events and refreshes every card.
    }
}