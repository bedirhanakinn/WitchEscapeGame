using UnityEngine;

public class EnemyThrower : MonoBehaviour
{
    [Header("References")]
    public Animator animator;
    public GameObject projectile;

    [Header("Settings")]
    public string playerTag = "Player";
    public float throwDelay = 0.2f; // sync with animation

    private bool hasThrown = false;

    private void Start()
    {
        // Make sure projectile starts disabled
        if (projectile != null)
        {
            projectile.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasThrown) return;

        if (other.CompareTag(playerTag))
        {
            StartThrow();
        }
    }

    void StartThrow()
    {
        hasThrown = true;

        // Trigger animation
        if (animator != null)
        {
            animator.SetTrigger("Throw");
        }

        // Delay actual throw (so it matches animation)
        Invoke(nameof(ActivateProjectile), throwDelay);
    }

    void ActivateProjectile()
    {
        if (projectile == null) return;

        projectile.SetActive(true);

        ProjectileArc arc = projectile.GetComponent<ProjectileArc>();
        if (arc != null)
        {
            arc.Launch();
        }
    }
}