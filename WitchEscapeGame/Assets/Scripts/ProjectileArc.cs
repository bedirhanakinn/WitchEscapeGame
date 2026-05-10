using UnityEngine;

public class ProjectileArc : MonoBehaviour
{
    [Header("Movement Settings")]
    public float horizontalDistance = 5f;

    [Header("Rise")]
    public float riseHeight = 2f;
    public float riseDuration = 0.3f;

    [Header("Glide Down")]
    public float fallDistance = 3f;
    public float fallDuration = 0.7f;

    private Vector3 startPos;
    private Vector3 peakPos;
    private Vector3 endPos;

    private float timer;
    private bool isMoving;

    private enum State
    {
        Rising,
        Falling
    }

    private State currentState;

    public void Launch()
    {
        startPos = transform.localPosition;

        // Position where projectile reaches its peak
        peakPos = startPos +
                  Vector3.left * horizontalDistance +
                  Vector3.up * riseHeight;

        // Final position after gliding down
        endPos = peakPos +
                 Vector3.left * fallDistance +
                 Vector3.down * riseHeight;

        timer = 0f;
        currentState = State.Rising;
        isMoving = true;
    }

    void Update()
    {
        if (!isMoving) return;

        timer += Time.deltaTime;

        if (currentState == State.Rising)
        {
            float t = timer / riseDuration;

            transform.localPosition = Vector3.Lerp(startPos, peakPos, t);

            if (t >= 1f)
            {
                timer = 0f;
                currentState = State.Falling;
            }
        }
        else if (currentState == State.Falling)
        {
            float t = timer / fallDuration;

            // Smooth glide downward
            t = Mathf.SmoothStep(0f, 1f, t);

            transform.localPosition = Vector3.Lerp(peakPos, endPos, t);

            if (t >= 1f)
            {
                DisableProjectile();
            }
        }
    }

    void DisableProjectile()
    {
        isMoving = false;
        gameObject.SetActive(false);

        // Reset for reuse
        transform.localPosition = startPos;
    }
}