using UnityEngine;

public class PotionFire : MonoBehaviour
{
    [Header("Explosion")]
    public GameObject explosionPrefab;

    [Header("Ground Detection")]
    public LayerMask groundLayer;
    public float groundCheckDistance = 10f;

    private bool exploded = false;

    void Update()
    {
        if (exploded)
            return;

        // Explode when reaching Y <= 0
        if (transform.position.y <= 0.5f)
        {
            Explode();
        }
    }

    void Explode()
    {
        exploded = true;

        Transform movingPlatform = null;

        // Detect the platform underneath
        RaycastHit2D hit = Physics2D.Raycast(
            transform.position,
            Vector2.down,
            groundCheckDistance,
            groundLayer
        );

        if (hit.collider != null)
        {
            // IMPORTANT:
            // Parent to the ACTUAL moving object,
            // not necessarily the root object.
            movingPlatform = hit.collider.transform;

            Debug.Log("Parenting explosion to: " + movingPlatform.name);
        }

        // Create explosion
        GameObject explosion = Instantiate(explosionPrefab);

        // Parent FIRST
        if (movingPlatform != null)
        {
            explosion.transform.SetParent(movingPlatform, true);
        }

        // THEN set world position
        explosion.transform.position = transform.position;

        // Reset rotation
        explosion.transform.rotation = Quaternion.identity;

        // Play particles manually
        ParticleSystem[] particleSystems =
            explosion.GetComponentsInChildren<ParticleSystem>();

        foreach (ParticleSystem ps in particleSystems)
        {
            ps.Clear();
            ps.Play();
        }

        // Destroy potion
        Destroy(gameObject);
    }

    // Debug ray visualization
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Gizmos.DrawLine(
            transform.position,
            transform.position + Vector3.down * groundCheckDistance
        );
    }
}