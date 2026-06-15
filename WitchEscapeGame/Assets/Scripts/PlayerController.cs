using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public Rigidbody2D rb;
    public float upwardSpeed = 6f;
    public float gravityScale = 2f;
    public float maxFallSpeed = -10f;

    [Header("Rotation")]
    public float fallRotationZ = -45f;
    public float fallRotationSpeed = 5f;
    public float recoverRotationSpeed = 15f;

    [Header("Projectile")]
    public GameObject projectilePrefab;
    public Transform projectileSpawnPoint;
    public float projectileForceX = 5f;
    public float projectileForceY = 3f;

    [Header("States")]
    public GameObject playerModel;
    public GameObject playerStumble;
    public GameObject playerDeath;

    [Header("Stumble Settings")]
    public float stumbleDuration = 7f;
    private bool isStumbling = false;
    private Coroutine stumbleCoroutine;

    [Header("Stumble Protection")]
    public float stumbleCooldown = 0.2f;
    private float lastStumbleTime = -1f;

    [Header("Ground Bounce")]
    public float groundY = 0.30f;
    public float bounceForce = 3f;
    private bool hasBounced = false;

    private bool isDead = false;

    private MainAnimation animationScript;
    private Vector2 startTouch;

    private PowerUpManager powerManager;

    void Start()
    {
        rb.gravityScale = gravityScale;

        animationScript =
            playerModel.GetComponent<MainAnimation>();

        powerManager =
            GetComponent<PowerUpManager>();

        powerManager.UpdateVisualState();
    }

    void Update()
    {
        if (isDead)
            return;

        HandleMovement();
        HandleRotation();
        HandleSwipe();
        HandleGroundBounce();
    }

    void HandleMovement()
    {
        bool isHolding =
            Input.GetMouseButton(0) ||
            Input.GetKey(KeyCode.Space);

        if (isHolding)
        {
            rb.velocity =
                new Vector2(
                    rb.velocity.x,
                    upwardSpeed
                );

            if (playerModel.activeInHierarchy)
                animationScript?.PlayUp();
        }
        else
        {
            if (rb.velocity.y < maxFallSpeed)
            {
                rb.velocity =
                    new Vector2(
                        rb.velocity.x,
                        maxFallSpeed
                    );
            }

            if (playerModel.activeInHierarchy)
                animationScript?.PlayDown();
        }
    }

    void HandleRotation()
    {
        bool isHolding =
            Input.GetMouseButton(0) ||
            Input.GetKey(KeyCode.Space);

        float targetZ =
            isHolding ? 0f : fallRotationZ;

        float currentSpeed =
            isHolding
            ? recoverRotationSpeed
            : fallRotationSpeed;

        Quaternion targetRotation =
            Quaternion.Euler(0f, 0f, targetZ);

        transform.rotation =
            Quaternion.Lerp(
                transform.rotation,
                targetRotation,
                currentSpeed * Time.deltaTime
            );
    }

    void HandleSwipe()
    {
        // ONLY allow throwing while normal model is active
        if (!playerModel.activeInHierarchy)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            startTouch = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0))
        {
            Vector2 delta =
                (Vector2)Input.mousePosition - startTouch;

            if (delta.x > 100f)
            {
                ThrowProjectile();
            }
        }
    }

    void ThrowProjectile()
    {
        // Extra safety check
        if (!playerModel.activeInHierarchy)
            return;

        GameObject proj =
            Instantiate(
                projectilePrefab,
                projectileSpawnPoint.position,
                Quaternion.identity
            );

        Rigidbody2D projRb =
            proj.GetComponent<Rigidbody2D>();

        projRb.velocity =
            new Vector2(
                projectileForceX,
                projectileForceY
            );

        animationScript?.PlayThrow();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead)
            return;

        // INVINCIBLE DURING POWER
        if (powerManager.IsPowerActive())
            return;

        if (collision.CompareTag("Death"))
        {
            Die();
        }
        else if (collision.CompareTag("Stumble"))
        {
            HandleStumble();
        }
    }

    void HandleStumble()
    {
        // IGNORE STUMBLE DURING POWER
        if (powerManager.IsPowerActive())
            return;

        // Prevent double trigger
        if (Time.time - lastStumbleTime < stumbleCooldown)
            return;

        lastStumbleTime = Time.time;

        if (isStumbling)
        {
            Die();
            return;
        }

        isStumbling = true;

        if (stumbleCoroutine != null)
            StopCoroutine(stumbleCoroutine);

        stumbleCoroutine =
            StartCoroutine(StumbleTimer());

        powerManager.UpdateVisualState();
    }

    IEnumerator StumbleTimer()
    {
        yield return new WaitForSeconds(stumbleDuration);

        isStumbling = false;

        powerManager.UpdateVisualState();
    }

    void Die()
    {
        // IGNORE DEATH DURING POWER
        if (powerManager.IsPowerActive())
            return;

        isDead = true;

        if (stumbleCoroutine != null)
        {
            StopCoroutine(stumbleCoroutine);
            stumbleCoroutine = null;
        }

        powerManager.UpdateVisualState();

        StartCoroutine(DeathSequence());
    }

    IEnumerator DeathSequence()
    {
        yield return new WaitForSeconds(0.5f);

        GameManager.instance.GameOver();
    }

    void HandleGroundBounce()
    {
        if (transform.position.y <= groundY)
        {
            if (!hasBounced)
            {
                rb.velocity =
                    new Vector2(
                        rb.velocity.x,
                        bounceForce
                    );

                hasBounced = true;

                // ONLY stumble if not powered
                if (!powerManager.IsPowerActive())
                {
                    HandleStumble();
                }
            }
        }
        else
        {
            hasBounced = false;
        }
    }

    /// <summary>
    /// Restores the player to a normal, controllable, alive state after a revive.
    /// Called by GameManager.ResumeFromRevive().
    ///
    /// Reverses everything Die() did: clears the dead/stumble flags, stops the
    /// death/stumble coroutines, resets physics so the player doesn't resume
    /// mid-fall, and forces the normal model active.
    /// </summary>
    public void ReviveToNormalState()
    {
        // Clear death + stumble flags
        isDead = false;
        isStumbling = false;

        // Stop any lingering death/stumble coroutines
        if (stumbleCoroutine != null)
        {
            StopCoroutine(stumbleCoroutine);
            stumbleCoroutine = null;
        }
        StopAllCoroutines();

        // Reset physics so the player doesn't resume mid-fall or rotated
        rb.velocity = Vector2.zero;
        transform.rotation = Quaternion.identity;
        hasBounced = false;

        // Explicitly force the normal model active (robust regardless of
        // how PowerUpManager.UpdateVisualState resolves state)
        if (playerModel != null) playerModel.SetActive(true);
        if (playerStumble != null) playerStumble.SetActive(false);
        if (playerDeath != null) playerDeath.SetActive(false);

        // Let the power manager re-sync any power-related visuals
        powerManager.UpdateVisualState();
    }

    public bool IsDead()
    {
        return isDead;
    }

    public bool IsStumbling()
    {
        return isStumbling;
    }
}