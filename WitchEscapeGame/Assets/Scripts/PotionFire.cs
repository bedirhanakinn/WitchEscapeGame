using UnityEngine;

public class PotionFire : MonoBehaviour
{
    [Header("Explosion")]
    public GameObject explosionPrefab;

    [Header("Ground Detection")]
    public LayerMask groundLayer;

    public float groundCheckDistance = 10f;
    public float overlapRadius = 1f;

    private bool exploded = false;

    void Update()
    {
        if (exploded)
            return;

        if (transform.position.y <= 0.5f)
        {
            Explode();
        }
    }

    void Explode()
    {
        exploded = true;

        Transform parentPlatform = null;

        // FIRST: Try raycast downward
        RaycastHit2D rayHit = Physics2D.Raycast(
            transform.position,
            Vector2.down,
            groundCheckDistance,
            groundLayer
        );

        if (rayHit.collider != null)
        {
            parentPlatform = rayHit.collider.transform.root;
        }
        else
        {
            // FALLBACK: overlap circle
            Collider2D overlapHit = Physics2D.OverlapCircle(
                transform.position,
                overlapRadius,
                groundLayer
            );

            if (overlapHit != null)
            {
                parentPlatform = overlapHit.transform.root;
            }
        }

        // Spawn explosion
        GameObject explosion = Instantiate(explosionPrefab);

        // Parent first
        if (parentPlatform != null)
        {
            explosion.transform.SetParent(parentPlatform);
        }

        // Then set position
        explosion.transform.position = transform.position;

        // Play particles
        ParticleSystem ps = explosion.GetComponentInChildren<ParticleSystem>();

        if (ps != null)
        {
            ps.Play();
        }

        Destroy(gameObject);
    }

    // Debug visuals
    private void OnDrawGizmosSelected()
    {
        // Ray
        Gizmos.color = Color.red;
        Gizmos.DrawLine(
            transform.position,
            transform.position + Vector3.down * groundCheckDistance
        );

        // Overlap
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(
            transform.position,
            overlapRadius
        );
    }
}