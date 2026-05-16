using UnityEngine;

public class CollectiblePickup : MonoBehaviour
{
    public CollectibleType collectibleType;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PowerUpManager manager =
            collision.GetComponent<PowerUpManager>();

        if (manager != null)
        {
            manager.Collect(collectibleType);

            Destroy(gameObject);
        }
    }
}