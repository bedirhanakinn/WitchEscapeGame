using UnityEngine;

public class EnemyThrower : MonoBehaviour
{
    [Header("References")]
    public GameObject projectile;

    [Header("Settings")]
    public string playerTag = "Player";
    public float throwDelay = 0.2f;

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

        // Delay actual throw
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