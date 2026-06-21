using UnityEngine;
using UnityEngine.UI;

public class AudioToggleButton : MonoBehaviour
{
    public enum AudioType
    {
        Music,
        SFX
    }

    [Header("What does this button control?")]
    public AudioType audioType;

    [Header("Sprites")]
    public Sprite onSprite;
    public Sprite offSprite;

    private Image buttonImage;

    private void Awake()
    {
        buttonImage = GetComponent<Image>();
    }

    private void Start()
    {
        UpdateSprite();
    }

    public void Toggle()
    {
        if (audioType == AudioType.Music)
        {
            SoundManager.Instance.ToggleMusic();
        }
        else
        {
            SoundManager.Instance.ToggleSFX();
        }

        UpdateSprite();
    }

    private void UpdateSprite()
    {
        bool enabledState;

        if (audioType == AudioType.Music)
        {
            enabledState = SoundManager.Instance.IsMusicEnabled();
        }
        else
        {
            enabledState = SoundManager.Instance.IsSFXEnabled();
        }

        buttonImage.sprite = enabledState ? onSprite : offSprite;
    }
}