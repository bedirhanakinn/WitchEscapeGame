using UnityEngine;

public class Vibrator : MonoBehaviour
{
    [Header("Vibration")]
    [Tooltip("Reserved for future use. Unity's built-in vibration uses the device's default duration.")]
    public float duration = 0.05f;

    private const string PREF_KEY = "VibrationEnabled";

    private void OnEnable()
    {
        Vibrate();
    }

    /// <summary>
    /// Vibrates the device if vibration is enabled.
    /// </summary>
    public static void Vibrate()
    {
        if (PlayerPrefs.GetInt(PREF_KEY, 1) == 0)
            return;

#if UNITY_ANDROID || UNITY_IOS
        Handheld.Vibrate();
#endif
    }

    /// <summary>
    /// Enables or disables vibration and saves the preference.
    /// </summary>
    public static void SetVibration(bool enabled)
    {
        PlayerPrefs.SetInt(PREF_KEY, enabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Returns whether vibration is currently enabled.
    /// </summary>
    public static bool IsEnabled()
    {
        return PlayerPrefs.GetInt(PREF_KEY, 1) == 1;
    }
}