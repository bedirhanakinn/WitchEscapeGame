using System;
using UnityEngine;

/// <summary>
/// Tracks coins collected during the current run for HUD and end-of-run displays.
/// Coins are added to permanent CurrencyManager immediately on pickup (no risk of loss),
/// but this counter shows the per-run total for visual feedback.
/// Reset by GameStart when a new run begins.
/// </summary>
public class RunCoinTracker : MonoBehaviour
{
    public static RunCoinTracker Instance { get; private set; }

    /// <summary>Fires with the new per-run count whenever a coin is collected this run.</summary>
    public event Action<int> OnRunCoinsChanged;

    public int CoinsThisRun { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void AddRunCoin(int amount)
    {
        if (amount <= 0) return;
        CoinsThisRun += amount;
        OnRunCoinsChanged?.Invoke(CoinsThisRun);
    }

    public void ResetRun()
    {
        CoinsThisRun = 0;
        OnRunCoinsChanged?.Invoke(0);
    }
}
