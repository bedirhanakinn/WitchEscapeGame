using UnityEngine;

public class ProjectileArcToTarget : MonoBehaviour
{
    private Vector2 startPos;
    private Vector2 targetPos;

    private float duration;
    private float arcHeight;
    private float worldSpeed;

    private float time = 0f;

    public void Initialize(Vector2 start, Vector2 target, float speed, float height, float worldSpeed)
    {
        startPos = start;
        targetPos = target;
        arcHeight = height;
        this.worldSpeed = worldSpeed;

        float distance = Vector2.Distance(start, target);
        duration = distance / speed;
    }

    void Update()
    {
        time += Time.deltaTime;

        float t = time / duration;

        if (t >= 1f)
        {
            transform.position = targetPos;
            Destroy(gameObject);
            return;
        }

        // Base movement (start → fixed target)
        Vector2 position = Vector2.Lerp(startPos, targetPos, t);

        // Arc
        float heightOffset = Mathf.Sin(t * Mathf.PI) * arcHeight;

        // ✅ Compensate for world moving left
        Vector2 worldOffset = Vector2.left * worldSpeed * time;

        transform.position = position + Vector2.up * heightOffset + worldOffset;
    }
}