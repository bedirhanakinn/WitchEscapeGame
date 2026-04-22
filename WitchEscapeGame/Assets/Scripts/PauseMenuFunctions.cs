using UnityEngine;

public class PauseMenuFunctions : MonoBehaviour
{
    private bool vibOn = true;
    private bool musicOn = true;
    private bool sfxOn = true;

    public void ToggleVibration()
    {
        vibOn = !vibOn;
        if(vibOn) Handheld.Vibrate();
        Debug.Log("Vibration: " + vibOn);
    }

    public void ToggleMusic()
    {
        musicOn = !musicOn;
        // Logic to mute music goes here
        Debug.Log("Music: " + musicOn);
    }

    public void ToggleSFX()
    {
        sfxOn = !sfxOn;
        // Logic to mute SFX goes here
        Debug.Log("SFX: " + sfxOn);
    }
}