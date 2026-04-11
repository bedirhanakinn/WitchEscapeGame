using UnityEngine;

public class EnemyHitbox : MonoBehaviour
{
    public EnemyController enemy;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            enemy.TriggerThrow(other.transform);
        }
    }
}