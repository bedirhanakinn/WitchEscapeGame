using UnityEngine;
using System.Collections;

public class SpriteChanger : MonoBehaviour
{
    [Header("Components")]
    public SpriteRenderer sr;

    [Header("Animation Settings")]
    public float frameRate = 0.1f;

    [Header("Idle")]
    public Sprite[] idle;

    [Header("Movement Animations")]
    public Sprite[] moveUp;
    public Sprite[] moveDown;
    public Sprite[] moveLeft;
    public Sprite[] moveRight;

    [Header("Stumble Animations")]
    public Sprite[] stumbleIdle;
    public Sprite[] stumbleMove;

    [Header("Throw & Death")]
    public Sprite[] throwAnim;
    public Sprite[] deathAnim;
    public float deathDuration = 1f;

    private bool stumbling;
    private Coroutine animRoutine;

    // --- Movement Animation ---
    public void PlayMove(int x, int y, bool stumble)
    {
        if (animRoutine != null) StopCoroutine(animRoutine);

        if (stumble)
        {
            animRoutine = StartCoroutine(PlayOnce(stumbleMove));
        }
        else
        {
            if (x > 0) animRoutine = StartCoroutine(PlayOnce(moveRight));
            else if (x < 0) animRoutine = StartCoroutine(PlayOnce(moveLeft));
            else if (y > 0) animRoutine = StartCoroutine(PlayOnce(moveDown)); // fixed
            else if (y < 0) animRoutine = StartCoroutine(PlayOnce(moveUp));   // fixed
        }
    }

    // --- Throw Animation ---
    public void PlayThrow()
    {
        if (animRoutine != null) StopCoroutine(animRoutine);
        animRoutine = StartCoroutine(PlayOnce(throwAnim));
    }

    // --- Enter Stumble ---
    public void EnterStumble()
    {
        stumbling = true;
        if (animRoutine != null) StopCoroutine(animRoutine);
        animRoutine = StartCoroutine(Loop(stumbleIdle));
    }

    // --- Exit Stumble ---
    public void ExitStumble()
    {
        stumbling = false;
        if (animRoutine != null) StopCoroutine(animRoutine);
        animRoutine = StartCoroutine(Loop(idle));
    }

    // --- Death Animation ---
    public void PlayDeath()
    {
        if (animRoutine != null) StopCoroutine(animRoutine);
        animRoutine = StartCoroutine(PlayOnce(deathAnim));
    }

    // --- Loop Animation ---
    private IEnumerator Loop(Sprite[] frames)
    {
        int i = 0;

        while (true)
        {
            sr.sprite = frames[i];
            i = (i + 1) % frames.Length;
            yield return new WaitForSeconds(frameRate);
        }
    }

    // --- Play Animation Once ---
    private IEnumerator PlayOnce(Sprite[] frames)
    {
        for (int i = 0; i < frames.Length; i++)
        {
            sr.sprite = frames[i];
            yield return new WaitForSeconds(frameRate);
        }

        // Resume correct idle after finishing
        if (stumbling)
            animRoutine = StartCoroutine(Loop(stumbleIdle));
        else
            animRoutine = StartCoroutine(Loop(idle));
    }
}