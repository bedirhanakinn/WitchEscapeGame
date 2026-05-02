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
    public float startDelay = 1f;

    // ==============================
    // UI / OBJECT CONTROL
    // ==============================
    [Header("Objects To Hide On Start")]
    public List<GameObject> objectsToHideOnStart;

    [Header("Objects To Disable On Tap")]
    public List<GameObject> objectsToDisable;

    [Header("Objects To Enable On Tap")]
    public List<GameObject> objectsToEnable;

    [Header("Objects To Enable After Movement Starts")]
    public List<GameObject> objectsToEnableAfterDelay;

    // allowedTapUI whitelist removed — no longer needed.
    // Blocking is now handled by IsPointerOverBlockedUI() below.

    // ==============================
    // EVENTS
    // ==============================
    [Header("Events")]
    [Tooltip("Fired the moment the player taps to start the game. Wire HUD UIFaders here so they fade in.")]
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
        {
            DisableWithChildren(obj);
        }
    }

    // ==============================
    // UPDATE
    // ==============================
    void Update()
    {
        if (!gameStarted && !isStarting)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (IsPointerOverBlockedUI())
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

    /// <summary>
    /// Returns true (blocks game start) when:
    ///   1. The tap lands on ANY UI element — catches Settings/Shop/Credits
    ///      buttons before their panels even open, with no whitelist needed.
    ///   2. A non-root menu is currently open (e.g. Settings panel is showing)
    ///      — prevents a tap on the background from starting the game while
    ///      a sub-menu is visible.
    /// </summary>
    bool IsPointerOverBlockedUI()
    {
        if (EventSystem.current == null)
            return false;

        // Block if pointer is over ANY UI element (buttons, panels, etc.)
        if (EventSystem.current.IsPointerOverGameObject())
            return true;

        // Also check real touch fingers (mobile)
        for (int i = 0; i < Input.touchCount; i++)
        {
            if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(i).fingerId))
                return true;
        }

        // Block if a sub-menu is open (Settings, Shop, Credits, Pause, etc.)
        // Safe-guarded in case UIManager.Instance isn't ready yet.
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

        foreach (GameObject obj in objectsToDisable)
        {
            DisableWithChildren(obj);
        }

        foreach (GameObject obj in objectsToEnable)
        {
            if (obj != null)
                obj.SetActive(true);
        }

        // Notify listeners (HUD UIFaders, etc.) that the game has started.
        onGameStarted?.Invoke();

        StartCoroutine(StartAfterDelay());
    }

    // ==============================
    // DELAY
    // ==============================
    IEnumerator StartAfterDelay()
    {
        yield return new WaitForSeconds(startDelay);

        foreach (GameObject obj in objectsToEnableAfterDelay)
        {
            if (obj != null)
                obj.SetActive(true);
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
        {
            Destroy(gameObject);
        }
    }

    // ==============================
    // FORCE DISABLE WITH CHILDREN
    // ==============================
    void DisableWithChildren(GameObject parent)
    {
        if (parent == null) return;

        parent.SetActive(false);

        foreach (Transform child in parent.transform)
        {
            child.gameObject.SetActive(false);
        }
    }
}