using UnityEngine;

public class SettingsAudioButtons : MonoBehaviour
{
    public void ToggleMusic()
    {
        SoundManager.Instance.ToggleMusic();
    }

    public void ToggleSFX()
    {
        SoundManager.Instance.ToggleSFX();
    }
}