using UnityEngine;
using UnityEngine.UI;

public class VibrationButton : MonoBehaviour
{
    [Header("Images")]
    public Image buttonImage;
    public Sprite onSprite;
    public Sprite offSprite;

    private void Start()
    {
        UpdateVisual();
    }

    public void ToggleVibration()
    {
        Vibrator.SetVibration(!Vibrator.IsEnabled());
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (buttonImage == null)
            return;

        buttonImage.sprite = Vibrator.IsEnabled() ? onSprite : offSprite;
    }
}