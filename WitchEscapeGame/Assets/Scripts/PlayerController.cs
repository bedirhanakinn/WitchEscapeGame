using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public Rigidbody2D rb;
    public float upwardSpeed = 6f;
    public float gravityScale = 2f;
    public float maxFallSpeed = -10f;

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

    [Header("UI")]
    public GameObject deathPanel;

    [Header("Ground Bounce")]
    public float groundY = 0.30f;
    public float bounceForce = 3f;
    private bool hasBounced = false;

    private bool isDead = false;

    private MainAnimation animationScript;

    private Vector2 startTouch;

    void Start()
    {
        rb.gravityScale = gravityScale;
        animationScript = playerModel.GetComponent<MainAnimation>();

        SetStateNormal();
    }

    void Update()
    {
        if (isDead) return;

        HandleMovement();
        HandleSwipe();
        HandleGroundBounce(); // Added ground bounce + stumble check
    }

    void HandleMovement()
    {
        bool isHolding = Input.GetMouseButton(0) || Input.GetKey(KeyCode.Space);

        if (isHolding)
        {
            rb.velocity = new Vector2(rb.velocity.x, upwardSpeed);

            if (playerModel.activeInHierarchy)
                animationScript?.PlayUp();
        }
        else
        {
            if (rb.velocity.y < maxFallSpeed)
                rb.velocity = new Vector2(rb.velocity.x, maxFallSpeed);

            if (playerModel.activeInHierarchy)
                animationScript?.PlayDown();
        }
    }

    void HandleSwipe()
    {
        if (Input.GetMouseButtonDown(0))
        {
            startTouch = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0))
        {
            Vector2 endTouch = Input.mousePosition;
            Vector2 delta = endTouch - startTouch;

            if (delta.x > 100f)
            {
                ThrowProjectile();
            }
        }
    }

    void ThrowProjectile()
    {
        GameObject proj = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.identity);

        Rigidbody2D projRb = proj.GetComponent<Rigidbody2D>();
        projRb.velocity = new Vector2(projectileForceX, projectileForceY);

        if (playerModel.activeInHierarchy)
            animationScript?.PlayThrow();
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDead) return;

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
        if (isStumbling)
        {
            Die();
            return;
        }

        isStumbling = true;

        if (stumbleCoroutine != null)
            StopCoroutine(stumbleCoroutine);

        stumbleCoroutine = StartCoroutine(StumbleTimer());

        SetStateStumble();
    }

    IEnumerator StumbleTimer()
    {
        yield return new WaitForSeconds(stumbleDuration);

        isStumbling = false;
        SetStateNormal();
    }

    void Die()
    {
        isDead = true;

        SetStateDeath();

        StartCoroutine(DeathSequence());
    }

    IEnumerator DeathSequence()
    {
        yield return new WaitForSeconds(0.5f);

        Time.timeScale = 0f;
        deathPanel.SetActive(true);
    }

    void SetStateNormal()
    {
        playerModel.SetActive(true);
        playerStumble.SetActive(false);
        playerDeath.SetActive(false);
    }

    void SetStateStumble()
    {
        playerModel.SetActive(false);
        playerStumble.SetActive(true);
        playerDeath.SetActive(false);
    }

    void SetStateDeath()
    {
        playerModel.SetActive(false);
        playerStumble.SetActive(false);
        playerDeath.SetActive(true);
    }

    void HandleGroundBounce()
{
    if (transform.position.y <= groundY)
    {
        if (!hasBounced)
        {
            rb.velocity = new Vector2(rb.velocity.x, bounceForce);
            hasBounced = true;

            // 👇 If already stumbling, Die() will be called automatically
            HandleStumble();
        }
    }
    else
    {
        hasBounced = false;
    }
}
}