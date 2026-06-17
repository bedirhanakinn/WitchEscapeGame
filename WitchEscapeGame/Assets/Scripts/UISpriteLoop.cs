using UnityEngine;
using UnityEngine.UI;

public class UISpriteLoop : MonoBehaviour
{
    public Sprite[] frames;
    public float frameRate = 5f;
    public bool loop = true;

    private Image image;
    private int index;
    private float timer;

    void Awake() { image = GetComponent<Image>(); }

    void OnEnable() { index = 0; timer = 0f; }

    void Update()
    {
        if (frames == null || frames.Length == 0 || image == null) return;

        timer += Time.unscaledDeltaTime;
        if (timer >= 1f / frameRate)
        {
            timer = 0f;
            index++;
            if (index >= frames.Length)
            {
                if (loop) index = 0;
                else { index = frames.Length - 1; return; }
            }
            image.sprite = frames[index];
        }
    }
}