using UnityEngine;

public class ProjectileArc : MonoBehaviour
{
    [Header("Arc Settings")]
    public float height = 2f;
    public float distance = 5f;
    public float duration = 1f;

    private Vector3 startLocalPos;
    private Vector3 endLocalPos;

    private float timer;
    private bool isMoving = false;

    public void Launch()
    {
        // Stay as child → inherit platform movement

        startLocalPos = transform.localPosition;

        // Always throw LEFT (local space)
        endLocalPos = startLocalPos + Vector3.left * distance;

        timer = 0f;
        isMoving = true;
    }

    void Update()
    {
        if (!isMoving) return;

        timer += Time.deltaTime;
        float t = timer / duration;

        if (t >= 1f)
        {
            DisableProjectile();
            return;
        }

        // Horizontal (LOCAL)
        Vector3 linear = Vector3.Lerp(startLocalPos, endLocalPos, t);

        // Arc
        float arc = height * 4 * (t - t * t);

        transform.localPosition = new Vector3(
            linear.x,
            linear.y + arc,
            linear.z
        );
    }

    void DisableProjectile()
    {
        isMoving = false;
        gameObject.SetActive(false);

        // OPTIONAL: reset position so next activation is clean
        transform.localPosition = startLocalPos;
    }
}