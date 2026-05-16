using UnityEngine;
using System.Collections;

public class EnableScreenShake : MonoBehaviour
{
    [Header("Shake Settings")]
    public Transform target;          // Usually your Camera
    public float duration = 0.15f;
    public float strength = 0.15f;

    [Header("Delay")]
    public float startDelay = 0f;

    private Vector3 originalPosition;

    private void OnEnable()
    {
        if (target == null && Camera.main != null)
            target = Camera.main.transform;

        StartCoroutine(Shake());
    }

    IEnumerator Shake()
    {
        // Optional delay before shake starts
        if (startDelay > 0f)
            yield return new WaitForSeconds(startDelay);

        originalPosition = target.localPosition;

        float timer = 0f;

        while (timer < duration)
        {
            float offsetX = Random.Range(-strength, strength);
            float offsetY = Random.Range(-strength, strength);

            target.localPosition = originalPosition + new Vector3(offsetX, offsetY, 0f);

            timer += Time.deltaTime;
            yield return null;
        }

        target.localPosition = originalPosition;
    }
}