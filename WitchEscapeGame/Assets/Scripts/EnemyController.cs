using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("References")]
    public Animator animator;
    public Transform throwPoint;
    public Transform targetPoint;
    public GameObject projectilePrefab;

    [Header("Throw Settings")]
    public float projectileSpeed = 10f;
    public float arcHeight = 2f;

    [Header("World Settings")]
    [Tooltip("Set this to your world movement speed (same as platform speed)")]
    public float worldSpeed = 0f;

    private bool hasThrown = false;

    public void TriggerThrow(Transform player)
    {
        if (hasThrown) return;

        hasThrown = true;

        // Play animation
        animator.SetTrigger("Throw");

        ThrowProjectile();
    }

    void ThrowProjectile()
    {
        // ✅ LOCK target position at the moment of throw
        Vector2 fixedTarget = targetPoint.position;

        GameObject proj = Instantiate(projectilePrefab, throwPoint.position, Quaternion.identity);

        ProjectileArcToTarget arc = proj.GetComponent<ProjectileArcToTarget>();

        if (arc != null)
        {
            arc.Initialize(
                start: throwPoint.position,
                target: fixedTarget,
                speed: projectileSpeed,
                height: arcHeight,
                worldSpeed: worldSpeed
            );
        }
    }
}