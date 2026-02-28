using UnityEngine;

public class PlatformMover : MonoBehaviour
{
    private PlatformManager manager;

    public void Initialize(PlatformManager platformManager)
    {
        manager = platformManager;
    }

    void Update()
    {
        if (!manager.GameStarted) return;

        transform.position += Vector3.left * manager.CurrentSpeed * Time.deltaTime;
    }
}