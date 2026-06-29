using UnityEngine;

public class Crow : MonoBehaviour
{
    [Header("Movement")]
    public float flySpeed = 5f;

    private bool flying = false;

    private void Update()
    {
        if (!flying)
            return;

        // Move left relative to the parent platform
        transform.localPosition += Vector3.left * flySpeed * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (flying)
            return;

        if (other.CompareTag("Player") ||
            other.CompareTag("Love") ||
            other.CompareTag("Frog"))
        {
            flying = true;
        }
    }
}