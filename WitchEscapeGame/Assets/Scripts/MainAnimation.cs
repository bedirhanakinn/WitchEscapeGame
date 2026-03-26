using UnityEngine;
using System.Collections;

public class MainAnimation : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;

    [Header("Animations")]
    public Sprite[] upSprites;
    public Sprite[] downSprites;
    public Sprite[] throwSprites;

    public float frameRate = 0.1f;

    private Coroutine currentAnimation;
    private string currentAnimationName = ""; // track current animation type

    public void PlayUp()
    {
        StartAnimation(upSprites, true, "Up");
    }

    public void PlayDown()
    {
        StartAnimation(downSprites, true, "Down");
    }

    public void PlayThrow()
    {
        StartAnimation(throwSprites, false, "Throw");
    }

    void StartAnimation(Sprite[] sprites, bool loop, string animationName)
    {
        // ✅ Only restart animation if it is different from the current one
        if (currentAnimationName == animationName)
            return;

        if (currentAnimation != null)
            StopCoroutine(currentAnimation);

        currentAnimationName = animationName;
        currentAnimation = StartCoroutine(PlaySprites(sprites, loop));
    }

    IEnumerator PlaySprites(Sprite[] sprites, bool loop)
    {
        do
        {
            for (int i = 0; i < sprites.Length; i++)
            {
                spriteRenderer.sprite = sprites[i];
                yield return new WaitForSeconds(frameRate);
            }
        } while (loop);
    }

    void OnDisable()
    {
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
            currentAnimation = null;
            currentAnimationName = "";
        }
    }
}