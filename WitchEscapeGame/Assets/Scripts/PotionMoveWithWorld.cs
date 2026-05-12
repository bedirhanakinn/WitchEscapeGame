using UnityEngine;

public class PotionMoveWithWorld : MonoBehaviour
{
    [Header("References")]
    public PlatformManager platformManager;

    void Start()
    {
        // Automatically find PlatformManager if not assigned
        if (platformManager == null)
        {
            GameObject spawner = GameObject.Find("Spawner");

            if (spawner != null)
            {
                platformManager = spawner.GetComponent<PlatformManager>();
            }
        }
    }

    void Update()
    {
        if (platformManager == null)
            return;

        // Move left using current world speed
        transform.position += Vector3.left *
                              platformManager.CurrentSpeed *
                              Time.deltaTime;
    }
}