using UnityEngine;
using System.Collections;

public class EnemyManager : MonoBehaviour
{
    [Header("Projectile")]
    public GameObject projectile;

    [Header("Throw Animation")]
    public Sprite[] throwSprites;
    public float throwFrameRate = 0.05f;

    [Header("Potion Animation")]
    public Sprite[] potionSprites;
    public float potionFrameRate = 0.05f;

    [Header("Transformations")]
    public GameObject loveChild;
    public GameObject frogChild;
    public float transformDelay = 0.5f;

    [Header("Reward")]
    public MonoBehaviour rewardGiver; // Drag your RewardGiver component here

    [Header("Tags")]
    public string playerTag = "Player";
    public string potionTag = "Potion";
    public string loveTag = "Love";
    public string frogTag = "Frog";

    private SpriteRenderer spriteRenderer;

    private bool hasThrown = false;
    private bool stateLocked = false;

    private Coroutine currentAnimation;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (projectile != null)
        {
            projectile.SetActive(false);
        }

        if (loveChild != null)
        {
            loveChild.SetActive(false);
        }

        if (frogChild != null)
        {
            frogChild.SetActive(false);
        }

        if (rewardGiver != null)
        {
            rewardGiver.enabled = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // PLAYER = THROW
        if (!hasThrown && !stateLocked && other.CompareTag(playerTag))
        {
            StartThrow();
            return;
        }

        // Ignore all further state changes once locked
        if (stateLocked)
            return;

        // POTION
        if (other.CompareTag(potionTag))
        {
            stateLocked = true;

            if (currentAnimation != null)
                StopCoroutine(currentAnimation);

            currentAnimation = StartCoroutine(PlayAnimation(
                potionSprites,
                potionFrameRate,
                true));

            return;
        }

        // LOVE
        if (other.CompareTag(loveTag))
        {
            stateLocked = true;
            StartCoroutine(TransformToLove());
            return;
        }

        // FROG
        if (other.CompareTag(frogTag))
        {
            stateLocked = true;
            StartCoroutine(TransformToFrog());
            return;
        }
    }

    private void StartThrow()
    {
        hasThrown = true;

        // THROW FIRST
        ActivateProjectile();

        // THEN PLAY ANIMATION
        if (throwSprites != null && throwSprites.Length > 0)
        {
            if (currentAnimation != null)
                StopCoroutine(currentAnimation);

            currentAnimation = StartCoroutine(PlayAnimation(
                throwSprites,
                throwFrameRate,
                false));
        }
    }

    private void ActivateProjectile()
    {
        if (projectile == null)
            return;

        projectile.SetActive(true);

        ProjectileArc arc = projectile.GetComponent<ProjectileArc>();

        if (arc != null)
        {
            arc.Launch();
        }
    }

    private IEnumerator PlayAnimation(
        Sprite[] sprites,
        float frameRate,
        bool enableRewardAfter)
    {
        if (sprites == null || sprites.Length == 0)
            yield break;

        for (int i = 0; i < sprites.Length; i++)
        {
            spriteRenderer.sprite = sprites[i];
            yield return new WaitForSeconds(frameRate);
        }

        // Freeze on last frame
        spriteRenderer.sprite = sprites[sprites.Length - 1];

        if (enableRewardAfter && rewardGiver != null)
        {
            rewardGiver.enabled = true;
        }
    }

    private IEnumerator TransformToLove()
    {
        yield return new WaitForSeconds(transformDelay);

        if (loveChild != null)
            loveChild.SetActive(true);

        spriteRenderer.enabled = false;
    }

    private IEnumerator TransformToFrog()
    {
        yield return new WaitForSeconds(transformDelay);

        if (frogChild != null)
            frogChild.SetActive(true);

        spriteRenderer.enabled = false;
    }
}