using UnityEngine;
using System.Collections;

public class SpriteChanger : MonoBehaviour
{
    [Header("References")]
    public SpriteRenderer spriteRenderer;

    [Header("Animation")]
    public Sprite[] sprites;
    public float frameRate = 0.1f;
    public bool loop = true;

    private Coroutine animationCoroutine;

    private void Awake()
    {
        // Auto assign if missing
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }

    private void OnEnable()
    {
        // Safety checks
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteChanger: No SpriteRenderer assigned!", this);
            return;
        }

        if (sprites == null || sprites.Length == 0)
        {
            Debug.LogError("SpriteChanger: No sprites assigned!", this);
            return;
        }

        animationCoroutine = StartCoroutine(PlaySprites());
    }

    IEnumerator PlaySprites()
    {
        do
        {
            for (int i = 0; i < sprites.Length; i++)
            {
                // Skip missing sprites
                if (sprites[i] != null)
                {
                    spriteRenderer.sprite = sprites[i];
                }

                yield return new WaitForSeconds(frameRate);
            }
        }
        while (loop);
    }

    private void OnDisable()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
    }
}