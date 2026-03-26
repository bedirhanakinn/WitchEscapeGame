using UnityEngine;
using System.Collections;

public class SpriteChanger : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;
    public Sprite[] sprites;
    public float frameRate = 0.1f;
    public bool loop = true;

    private Coroutine animationCoroutine;

    void OnEnable()
    {
        animationCoroutine = StartCoroutine(PlaySprites());
    }

    IEnumerator PlaySprites()
    {
        do
        {
            for (int i = 0; i < sprites.Length; i++)
            {
                spriteRenderer.sprite = sprites[i];
                yield return new WaitForSeconds(frameRate);
            }
        }
        while (loop);
    }

    void OnDisable()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
    }
}