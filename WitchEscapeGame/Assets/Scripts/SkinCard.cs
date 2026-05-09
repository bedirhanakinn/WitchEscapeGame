using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// One of these per card prefab. Shows the skin preview, price, and a
/// context-aware action button (Buy / Can't Afford / Equip / Equipped).
/// </summary>
public class SkinCard : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Image previewImage;
    [SerializeField] private TextMeshProUGUI displayNameText; // optional
    [SerializeField] private TextMeshProUGUI priceText;
    [SerializeField] private TextMeshProUGUI buttonLabel;
    [SerializeField] private Button actionButton;
    [SerializeField] private Image buttonBackground; // optional, for color states

    [Header("Button colors")]
    [SerializeField] private Color buyColor       = new Color(0.20f, 0.70f, 0.30f);
    [SerializeField] private Color cantAffordColor = new Color(0.50f, 0.50f, 0.50f);
    [SerializeField] private Color equipColor     = new Color(0.20f, 0.50f, 0.90f);
    [SerializeField] private Color equippedColor  = new Color(0.70f, 0.50f, 0.10f);

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

        bool owned     = SkinManager.Instance.IsOwned(skin.skinId);
        bool equipped  = SkinManager.Instance.IsEquipped(skin);
        bool canAfford = CurrencyManager.Instance.CanAfford(skin.price);

        if (equipped)
        {
            buttonLabel.text = "EQUIPPED";
            if (priceText != null) priceText.text = "";
            actionButton.interactable = false;
            SetButtonColor(equippedColor);
        }
        else if (owned)
        {
            buttonLabel.text = "EQUIP";
            if (priceText != null) priceText.text = "";
            actionButton.interactable = true;
            SetButtonColor(equipColor);
        }
        else
        {
            buttonLabel.text = "BUY";
            if (priceText != null) priceText.text = skin.price.ToString();
            actionButton.interactable = canAfford;
            SetButtonColor(canAfford ? buyColor : cantAffordColor);
        }
    }

    private void SetButtonColor(Color c)
    {
        if (buttonBackground != null) buttonBackground.color = c;
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