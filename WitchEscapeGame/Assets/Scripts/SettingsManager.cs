using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages persistent player settings: Vibration, Music, SFX.
/// Persists values via PlayerPrefs. Toggle buttons update visually via
/// assigned on/off sprites (swap the Image source) or tint (fallback).
/// Wire each button's onClick to the matching Toggle method.
/// </summary>
public class SettingsManager : MonoBehaviour
{
    // ── PlayerPrefs keys ───────────────────────────────────────────────
    private const string KEY_VIBRATION = "Setting_Vibration";
    private const string KEY_MUSIC     = "Setting_Music";
    private const string KEY_SFX       = "Setting_SFX";

    // ── Inspector references ───────────────────────────────────────────
    [Header("Toggle Buttons")]
    [SerializeField] private Button vibrationButton;
    [SerializeField] private Button musicButton;
    [SerializeField] private Button sfxButton;

    [Header("Button Images (reads the Image on the Button's GameObject)")]
    [SerializeField] private Image vibrationImage;
    [SerializeField] private Image musicImage;
    [SerializeField] private Image sfxImage;

    [Header("On / Off Sprites (optional — leave null to use tint only)")]
    [SerializeField] private Sprite vibrationOn;
    [SerializeField] private Sprite vibrationOff;
    [SerializeField] private Sprite musicOn;
    [SerializeField] private Sprite musicOff;
    [SerializeField] private Sprite sfxOn;
    [SerializeField] private Sprite sfxOff;

    [Header("Tint fallback (used when no sprites assigned)")]
    [SerializeField] private Color tintOn  = Color.white;
    [SerializeField] private Color tintOff = new Color(0.4f, 0.4f, 0.4f, 1f);

    // ── Runtime state ──────────────────────────────────────────────────
    public bool IsVibrationOn { get; private set; }
    public bool IsMusicOn     { get; private set; }
    public bool IsSFXOn       { get; private set; }

    // ── Singleton ──────────────────────────────────────────────────────
    public static SettingsManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        LoadSettings();
    }

    private void Start()
    {
        RefreshAllVisuals();
        ApplyAudioSettings();
    }

    // ── Public toggle methods — wire these to Button.onClick ───────────

    public void ToggleVibration()
    {
        IsVibrationOn = !IsVibrationOn;
        PlayerPrefs.SetInt(KEY_VIBRATION, IsVibrationOn ? 1 : 0);
        PlayerPrefs.Save();
        RefreshVisual(vibrationImage, IsVibrationOn, vibrationOn, vibrationOff);
    }

    public void ToggleMusic()
    {
        IsMusicOn = !IsMusicOn;
        PlayerPrefs.SetInt(KEY_MUSIC, IsMusicOn ? 1 : 0);
        PlayerPrefs.Save();
        RefreshVisual(musicImage, IsMusicOn, musicOn, musicOff);
        ApplyAudioSettings();
    }

    public void ToggleSFX()
    {
        IsSFXOn = !IsSFXOn;
        PlayerPrefs.SetInt(KEY_SFX, IsSFXOn ? 1 : 0);
        PlayerPrefs.Save();
        RefreshVisual(sfxImage, IsSFXOn, sfxOn, sfxOff);
        ApplyAudioSettings();
    }

    // ── Private helpers ────────────────────────────────────────────────

    private void LoadSettings()
    {
        // Default everything ON on first launch
        IsVibrationOn = PlayerPrefs.GetInt(KEY_VIBRATION, 1) == 1;
        IsMusicOn     = PlayerPrefs.GetInt(KEY_MUSIC,     1) == 1;
        IsSFXOn       = PlayerPrefs.GetInt(KEY_SFX,       1) == 1;
    }

    private void RefreshAllVisuals()
    {
        RefreshVisual(vibrationImage, IsVibrationOn, vibrationOn, vibrationOff);
        RefreshVisual(musicImage,     IsMusicOn,     musicOn,     musicOff);
        RefreshVisual(sfxImage,       IsSFXOn,       sfxOn,       sfxOff);
    }

    /// <summary>
    /// Swaps sprite if pairs are assigned; otherwise tints the image.
    /// </summary>
    private void RefreshVisual(Image img, bool isOn, Sprite onSprite, Sprite offSprite)
    {
        if (img == null) return;

        if (onSprite != null && offSprite != null)
            img.sprite = isOn ? onSprite : offSprite;
        else
            img.color = isOn ? tintOn : tintOff;
    }

    /// <summary>
    /// Stub — replace body with your AudioManager calls when ready.
    /// AudioListener.volume is a quick fallback for music muting.
    /// </summary>
    private void ApplyAudioSettings()
    {
        // Example minimal implementation — swap for your AudioManager:
        // AudioListener.volume = IsMusicOn ? 1f : 0f;
        //
        // For SFX you'll typically call something like:
        // AudioManager.Instance.SetSFXVolume(IsSFXOn ? 1f : 0f);
    }
}
