using System;
using UnityEngine;

/// <summary>
/// Stub currency system. Persists to PlayerPrefs.
/// Wire AddCoins() to whatever earns coins later (run rewards, pickups, IAP, etc).
/// </summary>
public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }
    public event Action OnCurrencyChanged;

    private const string COINS_KEY = "currency_coins";

    public int Coins { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        Coins = PlayerPrefs.GetInt(COINS_KEY, 0);
    }

    public bool CanAfford(int amount) => Coins >= amount;

    public bool TrySpend(int amount)
    {
        if (amount < 0) return false;
        if (!CanAfford(amount)) return false;
        Coins -= amount;
        Save();
        OnCurrencyChanged?.Invoke();
        return true;
    }

    public void AddCoins(int amount)
    {
        if (amount <= 0) return;
        Coins += amount;
        Save();
        OnCurrencyChanged?.Invoke();
    }

    private void Save()
    {
        PlayerPrefs.SetInt(COINS_KEY, Coins);
        PlayerPrefs.Save();
    }

    // --- Debug helpers (right-click component header in Inspector) ---
    [ContextMenu("Debug: Add 100 Coins")]
    private void DebugAdd100() => AddCoins(100);

    [ContextMenu("Debug: Add 1000 Coins")]
    private void DebugAdd1000() => AddCoins(1000);

    [ContextMenu("Debug: Reset Coins")]
    private void DebugReset()
    {
        Coins = 0;
        Save();
        OnCurrencyChanged?.Invoke();
    }
}
