using System;
using UnityEngine;

public class SkinManager : MonoBehaviour
{
    public static SkinManager Instance { get; private set; }
    public event Action<SkinData> OnSkinEquipped;

    [SerializeField] private SkinDatabase database;

    public SkinDatabase Database => database;
    public SkinData CurrentSkin { get; private set; }

    private const string EQUIPPED_KEY = "skin_equipped";
    private static string OwnedKey(string id) => $"skin_owned_{id}";

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (database == null)
        {
            Debug.LogError("SkinManager: No SkinDatabase assigned.");
            return;
        }

        // Mark default-owned skins on first launch
        foreach (var skin in database.skins)
        {
            if (skin != null && skin.ownedByDefault && !IsOwned(skin.skinId))
                SetOwned(skin.skinId, true);
        }

        // Load equipped skin, fall back to default if missing/unowned
        string equippedId = PlayerPrefs.GetString(EQUIPPED_KEY, "");
        SkinData loaded = database.GetById(equippedId);
        if (loaded == null || !IsOwned(loaded.skinId))
            loaded = database.defaultSkin;

        CurrentSkin = loaded;
    }

    public bool IsOwned(string skinId) =>
        PlayerPrefs.GetInt(OwnedKey(skinId), 0) == 1;

    public bool IsEquipped(SkinData skin) =>
        skin != null && CurrentSkin != null && CurrentSkin.skinId == skin.skinId;

    /// <summary>Buys + auto-equips the skin if the player can afford it. Returns true on success.</summary>
    public bool TryBuy(SkinData skin)
    {
        if (skin == null) return false;
        if (IsOwned(skin.skinId)) return false;
        if (CurrencyManager.Instance == null) return false;
        if (!CurrencyManager.Instance.TrySpend(skin.price)) return false;

        SetOwned(skin.skinId, true);
        Equip(skin);
        return true;
    }

    public void Equip(SkinData skin)
    {
        if (skin == null || !IsOwned(skin.skinId)) return;
        CurrentSkin = skin;
        PlayerPrefs.SetString(EQUIPPED_KEY, skin.skinId);
        PlayerPrefs.Save();
        OnSkinEquipped?.Invoke(skin);
    }

    private void SetOwned(string skinId, bool owned)
    {
        PlayerPrefs.SetInt(OwnedKey(skinId), owned ? 1 : 0);
        PlayerPrefs.Save();
    }

    [ContextMenu("Debug: Reset All Skin Data")]
    private void DebugResetAll()
    {
        if (database == null) return;
        foreach (var s in database.skins)
            if (s != null) PlayerPrefs.DeleteKey(OwnedKey(s.skinId));
        PlayerPrefs.DeleteKey(EQUIPPED_KEY);
        Awake();
        OnSkinEquipped?.Invoke(CurrentSkin);
    }
}
