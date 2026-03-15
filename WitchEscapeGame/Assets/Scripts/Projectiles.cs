using UnityEngine;

public class Projectile : MonoBehaviour
{
    public Rigidbody2D rb;

    public float xForce = 3f;
    public float yForce = 6f;

    void Start()
    {
        rb.AddForce(new Vector2(xForce, yForce), ForceMode2D.Impulse);
        Destroy(gameObject,5f);
    }
}