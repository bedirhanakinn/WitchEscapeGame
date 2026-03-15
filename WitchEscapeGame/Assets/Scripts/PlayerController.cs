using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Grid Setup")]
    public Transform[] gridPoints; // Assign 9 points in inspector
    private Transform[,] grid = new Transform[3, 3];

    [Header("Movement")]
    public float moveDuration = 0.15f;
    private int gridX = 1;
    private int gridY = 1;
    private bool isMoving;

    [Header("Shake")]
    public float shakeAmount = 0.15f;
    public float shakeTime = 0.2f;

    [Header("Projectile")]
    public GameObject projectilePrefab;
    public Transform firePoint;

    [Header("Stumble/Health")]
    private bool isStumbling;
    private float stumbleTimer;
    public float stumbleDuration = 7f;

    [Header("Swipe Input")]
    private Vector2 touchStart;
    public float swipeThreshold = 70f;

    private SpriteChanger spriteChanger;

    void Start()
    {
        spriteChanger = GetComponent<SpriteChanger>();

        // Fill the 2D grid
        int index = 0;
        for (int y = 0; y < 3; y++)
        {
            for (int x = 0; x < 3; x++)
            {
                grid[x, y] = gridPoints[index];
                index++;
            }
        }

        // Start at center
        transform.position = grid[gridX, gridY].position;
    }

    void Update()
    {
        HandleKeyboardInput();
        HandleSwipeInput();
        HandleStumbleTimer();
    }

    // --- Keyboard Input ---
    void HandleKeyboardInput()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow)) Move(1, 0);
        if (Input.GetKeyDown(KeyCode.LeftArrow)) Move(-1, 0);
        if (Input.GetKeyDown(KeyCode.UpArrow)) Move(0, -1);    // UP now decreases Y (visual up)
        if (Input.GetKeyDown(KeyCode.DownArrow)) Move(0, 1);   // DOWN increases Y

        if (Input.GetKeyDown(KeyCode.Space))
            ThrowProjectile();
    }

    // --- Touch & Mouse Input ---
    void HandleSwipeInput()
    {
        Vector2 delta = Vector2.zero;
        bool swipeDetected = false;

        // Touch Input
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
                touchStart = touch.position;

            if (touch.phase == TouchPhase.Ended)
            {
                delta = touch.position - touchStart;
                swipeDetected = true;
            }
        }
        // Mouse Input
        else
        {
            if (Input.GetMouseButtonDown(0))
                touchStart = Input.mousePosition;

            if (Input.GetMouseButtonUp(0))
            {
                delta = (Vector2)Input.mousePosition - touchStart;
                swipeDetected = true;
            }
        }

        if (!swipeDetected) return;

        if (delta.magnitude < swipeThreshold)
        {
            // Tap/click → throw projectile
            ThrowProjectile();
            return;
        }

        // Determine swipe direction
        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        {
            if (delta.x > 0) Move(1, 0);
            else Move(-1, 0);
        }
        else
        {
            if (delta.y > 0) Move(0, -1);    // Swipe UP → move visually up
            else Move(0, 1);                 // Swipe DOWN → move visually down
        }
    }

    // --- Movement ---
    void Move(int xDir, int yDir)
    {
        int targetX = gridX + xDir;
        int targetY = gridY + yDir;

        // Check boundaries
        if (targetX < 0 || targetX > 2 || targetY < 0 || targetY > 2)
        {
            StartCoroutine(Shake());
            TriggerStumble();
            return;
        }

        // Update grid coordinates
        gridX = targetX;
        gridY = targetY;

        // Stop any previous movement
        StopAllCoroutines();
        StartCoroutine(MoveTo(grid[gridX, gridY].position));

        // Play movement animation
        spriteChanger.PlayMove(xDir, yDir, isStumbling);
    }

    IEnumerator MoveTo(Vector3 target)
    {
        isMoving = true;
        Vector3 start = transform.position;
        float t = 0;

        while (t < moveDuration)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(start, target, t / moveDuration);
            yield return null;
        }

        transform.position = target;
        isMoving = false;
    }

    // --- Shake for invalid movement ---
    IEnumerator Shake()
    {
        Vector3 start = transform.position;
        float t = 0;

        while (t < shakeTime)
        {
            t += Time.deltaTime;
            Vector3 offset = Random.insideUnitCircle * shakeAmount;
            transform.position = start + offset;
            yield return null;
        }

        transform.position = start;
    }

    // --- Stumble System ---
    void TriggerStumble()
    {
        if (isStumbling)
        {
            Die();
            return;
        }

        isStumbling = true;
        stumbleTimer = stumbleDuration;
        spriteChanger.EnterStumble();
    }

    void HandleStumbleTimer()
    {
        if (!isStumbling) return;

        stumbleTimer -= Time.deltaTime;
        if (stumbleTimer <= 0)
        {
            isStumbling = false;
            spriteChanger.ExitStumble();
        }
    }

    // --- Death ---
    void Die()
    {
        StartCoroutine(DeathSequence());
    }

    IEnumerator DeathSequence()
    {
        spriteChanger.PlayDeath();
        yield return new WaitForSeconds(spriteChanger.deathDuration);
        GameManager.instance.GameOver();
    }

    // --- Projectile ---
    void ThrowProjectile()
    {
        Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        spriteChanger.PlayThrow();
    }

    // --- Collisions ---
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Stumble"))
        {
            Destroy(other.gameObject);
            TriggerStumble();
        }

        if (other.CompareTag("Death"))
        {
            Die();
        }
    }
}