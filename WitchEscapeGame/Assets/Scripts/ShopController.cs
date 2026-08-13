using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Attach to the Shop UIMenu panel. Spawns one SkinCard per skin in the database,
/// keeps the currency display and all cards in sync via events, and applies
/// tiered lock gating over paid skins.
/// </summary>
public class ShopController : MonoBehaviour
{
    [SerializeField] private SkinDatabase database;
    [SerializeField] private Transform cardContainer;       // ScrollRect's Content
    [SerializeField] private SkinCard cardPrefab;
    [SerializeField] private TextMeshProUGUI currencyText;

    [Header("Tier Gating")]
    [SerializeField] private int skinsPerTier = 3;

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

        ApplyLockStates();               // decide locks first
        foreach (var c in cards) c.Refresh();
    }

    /// <summary>
    /// Tiered unlock. Paid skins are grouped in database order into sets of
    /// `skinsPerTier`. Tier 0 is always viewable. Each later tier unlocks only
    /// when every paid skin in the previous tier is owned. Free skins never lock.
    /// The "needed" count = unowned paid skins between the current frontier tier
    /// and the card's own tier, used for the dynamic lock message.
    /// </summary>
    private void ApplyLockStates()
    {
        if (SkinManager.Instance == null || database == null) return;

        // Non-null skins in order. Matches the `cards` list 1:1.
        var ordered = new List<SkinData>();
        foreach (var s in database.skins)
            if (s != null) ordered.Add(s);

        // Paid skins only, in order.
        var paid = new List<SkinData>();
        foreach (var s in ordered)
            if (!IsFreeSkin(s)) paid.Add(s);

        // How many complete leading tiers are fully owned.
        int totalTiers = (paid.Count + skinsPerTier - 1) / skinsPerTier;
        int fullyOwnedLeadingTiers = 0;
        for (int t = 0; t < totalTiers; t++)
        {
            bool allOwned = true;
            int end = Mathf.Min((t + 1) * skinsPerTier, paid.Count);
            for (int i = t * skinsPerTier; i < end; i++)
            {
                if (!SkinManager.Instance.IsOwned(paid[i].skinId)) { allOwned = false; break; }
            }
            if (allOwned) fullyOwnedLeadingTiers++;
            else break;
        }
        int maxViewableTier = fullyOwnedLeadingTiers; // tiers 0..maxViewableTier viewable

        for (int idx = 0; idx < ordered.Count && idx < cards.Count; idx++)
        {
            SkinData s = ordered[idx];

            if (IsFreeSkin(s))
            {
                cards[idx].SetLocked(false, 0);
                continue;
            }

            int tier = paid.IndexOf(s) / skinsPerTier;
            bool locked = tier > maxViewableTier;

            int needed = 0;
            if (locked)
            {
                // Count unowned paid skins in tiers [maxViewableTier .. tier-1].
                int start = maxViewableTier * skinsPerTier;
                int end = Mathf.Min(tier * skinsPerTier, paid.Count);
                for (int i = start; i < end; i++)
                    if (!SkinManager.Instance.IsOwned(paid[i].skinId)) needed++;
            }

            cards[idx].SetLocked(locked, needed);
        }
    }

    private bool IsFreeSkin(SkinData s)
    {
        return s.ownedByDefault || s.price <= 0;
    }
}
