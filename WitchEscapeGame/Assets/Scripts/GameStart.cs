using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class GameStart : MonoBehaviour
{
    // ==============================
    // MOVEMENT SETTINGS
    // ==============================
    [Header("Movement Settings")]
    public float moveSpeed = 10f;
    public float despawnX = -50f;

    // ==============================
    // START DELAY
    // ==============================
    [Header("Start Delay")]
    [Tooltip("Total time from tap to game movement starting.")]
    public float startDelay = 1f;

    [Tooltip("Time after tap before switching from Looking Up to Whistling. Must be less than startDelay.")]
    public float witchSwapDelay = 0.5f;

    // ==============================
    // UI / OBJECT CONTROL
    // ==============================
    [Header("Objects To Hide On Start (scene load)")]
    public List<GameObject> objectsToHideOnStart;

    [Header("Objects To Disable On Tap (immediately)")]
    public List<GameObject> objectsToDisable;

    [Header("Objects To Enable On Tap (immediately)")]
    public List<GameObject> objectsToEnable;

    [Header("Witch Swap (after witchSwapDelay seconds)")]
    [Tooltip("Hide these after witchSwapDelay. e.g. Looking Up Witch.")]
    public List<GameObject> objectsToHideAfterSwap;

    [Tooltip("Show these after witchSwapDelay. e.g. Whistling Witch.")]
    public List<GameObject> objectsToShowAfterSwap;

    [Header("Objects To Enable After Full Delay")]
    public List<GameObject> objectsToEnableAfterDelay;

    [Header("Objects To Disable After Full Delay")]
    [Tooltip("Hidden when movement starts. Use this to hide Whistling Witch before gameplay begins.")]
    public List<GameObject> objectsToDisableAfterDelay;

    // ==============================
    // EVENTS
    // ==============================
    [Header("Events")]
    [Tooltip("Fired the moment the player taps to start the game.")]
    public UnityEvent onGameStarted;

    // ==============================
    // INTERNAL STATE
    // ==============================
    private bool gameStarted = false;
    private bool canMove = false;
    private bool isStarting = false;

    // ==============================
    // START
    // ==============================
    void Start()
    {
        foreach (GameObject obj in objectsToHideOnStart)
            DisableWithChildren(obj);
    }

    // ==============================
    // UPDATE
    // ==============================
    void Update()
    {
        if (!gameStarted && !isStarting)
        {
            if (
                Input.GetMouseButtonDown(0) ||
                Input.GetKeyDown(KeyCode.W) ||
                Input.GetKeyDown(KeyCode.D) ||
                Input.GetKeyDown(KeyCode.Space) ||
                Input.GetKeyDown(KeyCode.UpArrow) ||
                Input.GetKeyDown(KeyCode.RightArrow)
            )
            {
                // Only block mouse/touch input when clicking UI
                if (Input.GetMouseButtonDown(0) && IsPointerOverBlockedUI())
                    return;

                StartGame();
            }
        }
        else if (canMove)
        {
            Move();
        }
    }

    // ==============================
    // CHECK UI INPUT
    // ==============================
    bool IsPointerOverBlockedUI()
    {
        if (EventSystem.current == null)
            return false;

        if (EventSystem.current.IsPointerOverGameObject())
            return true;

        for (int i = 0; i < Input.touchCount; i++)
        {
            if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(i).fingerId))
                return true;
        }

        if (UIManager.Instance != null)
        {
            UIManager.MenuId current = UIManager.Instance.Current;
            if (current != UIManager.MenuId.None && current != UIManager.MenuId.MainMenu)
                return true;
        }

        return false;
    }

    // ==============================
    // START GAME
    // ==============================
    void StartGame()
    {
        if (isStarting) return;

        isStarting = true;
        gameStarted = true;

        // Immediately disable (Burning Witch, etc.)
        foreach (GameObject obj in objectsToDisable)
            DisableWithChildren(obj);

        // Immediately enable (Looking Up Witch)
        foreach (GameObject obj in objectsToEnable)
        {
            if (obj != null) obj.SetActive(true);
        }

        onGameStarted?.Invoke();

        StartCoroutine(StartAfterDelay());
    }

    // ==============================
    // DELAY SEQUENCE
    // ==============================
    IEnumerator StartAfterDelay()
    {
        // Phase 1: Wait for witch swap (Looking Up → Whistling)
        float swapTime = Mathf.Min(witchSwapDelay, startDelay);
        yield return new WaitForSeconds(swapTime);

        // Hide Looking Up Witch, show Whistling Witch
        foreach (GameObject obj in objectsToHideAfterSwap)
            if (obj != null) obj.SetActive(false);

        foreach (GameObject obj in objectsToShowAfterSwap)
            if (obj != null) obj.SetActive(true);

        // Phase 2: Wait for remaining time before movement starts
        float remainingDelay = startDelay - swapTime;
        if (remainingDelay > 0f)
            yield return new WaitForSeconds(remainingDelay);

        // Enable delayed objects (HUD, etc.)
        foreach (GameObject obj in objectsToEnableAfterDelay)
        {
            if (obj != null) obj.SetActive(true);
        }

        // Hide any remaining menu objects (e.g. Whistling Witch)
        foreach (GameObject obj in objectsToDisableAfterDelay)
        {
            if (obj != null) obj.SetActive(false);
        }

        canMove = true;
    }

    // ==============================
    // MOVEMENT
    // ==============================
    void Move()
    {
        transform.position += Vector3.left * moveSpeed * Time.deltaTime;

        if (transform.position.x <= despawnX)
            Destroy(gameObject);
    }

    // ==============================
    // FORCE DISABLE WITH CHILDREN
    // ==============================
    void DisableWithChildren(GameObject parent)
    {
        if (parent == null) return;
        parent.SetActive(false);
        foreach (Transform child in parent.transform)
            child.gameObject.SetActive(false);
    }
}