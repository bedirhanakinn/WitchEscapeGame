using UnityEngine;

public class PotionFire : MonoBehaviour
{
    [Header("Explosion")]
    public GameObject explosionPrefab;

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

        Instantiate(
            explosionPrefab,
            transform.position,
            Quaternion.identity
        );

        Destroy(gameObject);
    }
}