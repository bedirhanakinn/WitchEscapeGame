using UnityEngine;

public class Wobble : MonoBehaviour
{
    [Header("Wobble Settings")]
    public Vector3 wobbleAxis = new Vector3(0, 0, 1); // axis to wobble around
    public float angleRange = 15f; // max rotation in degrees
    public float speed = 2f; // wobble speed

    private Quaternion startRotation;

    void Start()
    {
        startRotation = transform.localRotation;
    }

    void Update()
    {
        float angle = Mathf.Sin(Time.time * speed) * angleRange;
        transform.localRotation = startRotation * Quaternion.AngleAxis(angle, wobbleAxis.normalized);
    }
}