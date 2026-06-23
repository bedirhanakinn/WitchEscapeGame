using UnityEngine;

public class ParallaxLooper : MonoBehaviour
{
    [Header("Background Pieces")]
    public Transform bg1;
    public Transform bg2;
    public Transform bg3;

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
        bgWidth = sr.bounds.size.x;

        bg1.position = new Vector3(startX, bg1.position.y, bg1.position.z);

        bg2.position = new Vector3(
            startX + bgWidth,
            bg2.position.y,
            bg2.position.z
        );

        bg3.position = new Vector3(
            startX + (bgWidth * 2f),
            bg3.position.y,
            bg3.position.z
        );
    }

    void Update()
    {
        currentSpeed += acceleration * Time.deltaTime;
        currentSpeed = Mathf.Clamp(currentSpeed, 0, maxSpeed);

        float move = currentSpeed * speedMultiplier * Time.deltaTime;

        bg1.position += Vector3.left * move;
        bg2.position += Vector3.left * move;
        bg3.position += Vector3.left * move;

        LoopBackground(bg1);
        LoopBackground(bg2);
        LoopBackground(bg3);
    }

    void LoopBackground(Transform bg)
    {
        if (bg.position.x <= startX - bgWidth)
        {
            float furthestX = Mathf.Max(
                bg1.position.x,
                Mathf.Max(bg2.position.x, bg3.position.x)
            );

            bg.position = new Vector3(
                furthestX + bgWidth,
                bg.position.y,
                bg.position.z
            );
        }
    }
}