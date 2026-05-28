using UnityEngine;

public class ParallaxLooper : MonoBehaviour
{
    [Header("Background Pieces")]
    public Transform bg1;
    public Transform bg2;

    [Header("Movement")]
    public float baseSpeed = 2f;
    public float speedMultiplier = 1f;

    [Header("Acceleration")]
    public float acceleration = 0.1f;
    public float maxSpeed = 20f;

    [Header("Start Position")]
    public float startX = -30f;

    private float currentSpeed;
    private float bgWidth;

    void Start()
    {
        currentSpeed = baseSpeed;

        SpriteRenderer sr = bg1.GetComponent<SpriteRenderer>();

        // Automatically get sprite width in world units
        bgWidth = sr.bounds.size.x;

        // Initial positions
        bg1.position = new Vector3(startX, bg1.position.y, bg1.position.z);

        bg2.position = new Vector3(
            startX + bgWidth,
            bg2.position.y,
            bg2.position.z
        );
    }

    void Update()
    {
        // Increase speed over time
        currentSpeed += acceleration * Time.deltaTime;
        currentSpeed = Mathf.Clamp(currentSpeed, 0, maxSpeed);

        float move = currentSpeed * speedMultiplier * Time.deltaTime;

        // Move backgrounds left
        bg1.position += Vector3.left * move;
        bg2.position += Vector3.left * move;

        // Loop bg1
        if (bg1.position.x <= startX - bgWidth)
        {
            bg1.position = new Vector3(
                bg2.position.x + bgWidth,
                bg1.position.y,
                bg1.position.z
            );
        }

        // Loop bg2
        if (bg2.position.x <= startX - bgWidth)
        {
            bg2.position = new Vector3(
                bg1.position.x + bgWidth,
                bg2.position.y,
                bg2.position.z
            );
        }
    }
}