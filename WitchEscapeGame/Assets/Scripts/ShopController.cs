using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Attach to the Shop UIMenu panel. Spawns one SkinCard per skin in the database,
/// keeps the currency display and all cards in sync via events.
/// </summary>
public class ShopController : MonoBehaviour
{
    [SerializeField] private SkinDatabase database;
    [SerializeField] private Transform cardContainer;       // ScrollRect's Content
    [SerializeField] private SkinCard cardPrefab;
    [SerializeField] private TextMeshProUGUI currencyText;

    private readonly List<SkinCard> cards = new List<SkinCard>();
    private bool initialized;

    void OnEnable()
    {
        if (!initialized) BuildCards();
        Subscribe();
        RefreshAll();
    }

    void OnDisable()
    {
        Unsubscribe();
    }

    private void BuildCards()
    {
        if (database == null || cardPrefab == null || cardContainer == null)
        {
            Debug.LogError("ShopController: missing references.");
            return;
        }

        foreach (var skin in database.skins)
        {
            if (skin == null) continue;
            var card = Instantiate(cardPrefab, cardContainer);
            card.Initialize(skin);
            cards.Add(card);
        }
        initialized = true;
    }

    private void Subscribe()
    {
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnCurrencyChanged += RefreshAll;
        if (SkinManager.Instance != null)
            SkinManager.Instance.OnSkinEquipped += OnSkinEquippedChanged;
    }

    private void Unsubscribe()
    {
        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.OnCurrencyChanged -= RefreshAll;
        if (SkinManager.Instance != null)
            SkinManager.Instance.OnSkinEquipped -= OnSkinEquippedChanged;
    }

    private void OnSkinEquippedChanged(SkinData _) => RefreshAll();

    private void RefreshAll()
    {
        if (currencyText != null && CurrencyManager.Instance != null)
            currencyText.text = CurrencyManager.Instance.Coins.ToString();
        foreach (var c in cards) c.Refresh();
    }
}
