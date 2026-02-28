using UnityEngine;

public class GameStart : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 10f;    // Units per second
    public float despawnX = -50f;    // X position where it gets destroyed

    private bool gameStarted = false;

    void Update()
    {
        if (!gameStarted)
        {
            // Wait for first tap
            if (Input.GetMouseButtonDown(0))
            {
                gameStarted = true;
            }
        }
        else
        {
            // Move the existing prefab left
            transform.position += Vector3.left * moveSpeed * Time.deltaTime;

            // Delete if off-screen
            if (transform.position.x <= despawnX)
            {
                Destroy(gameObject);
            }
        }
    }
}