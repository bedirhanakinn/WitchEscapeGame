using UnityEngine;
using System.Collections;

public class EnableScreenShake : MonoBehaviour
{
    [Header("Shake Settings")]
    public Transform target; // Usually your Camera
    public float duration = 0.15f;
    public float strength = 0.15f;

    [Header("Delay")]
    public float startDelay = 0f;

    private Coroutine shakeRoutine;
    private Vector3 savedPosition;

    private void OnEnable()
    {
        // Auto-find Main Camera if none assigned
        if (target == null && Camera.main != null)
            target = Camera.main.transform;

        if (target == null)
            return;

        // Save clean original position
        savedPosition = target.localPosition;

        // Stop any existing shake
        if (shakeRoutine != null)
            StopCoroutine(shakeRoutine);

        // Force reset before starting new shake
        target.localPosition = savedPosition;

        // Start shake
        shakeRoutine = StartCoroutine(Shake());
    }

    IEnumerator Shake()
    {
        // Optional delay
        if (startDelay > 0f)
            yield return new WaitForSeconds(startDelay);

        float timer = 0f;

        while (timer < duration)
        {
            float offsetX = Random.Range(-strength, strength);
            float offsetY = Random.Range(-strength, strength);

            target.localPosition = savedPosition + new Vector3(offsetX, offsetY, 0f);

            timer += Time.deltaTime;

            yield return null;
        }

        // PERFECT reset
        target.localPosition = savedPosition;

        shakeRoutine = null;
    }

    private void OnDisable()
    {
        // Safety reset if disabled during shake
        if (target != null)
            target.localPosition = savedPosition;
    }
}