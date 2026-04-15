using UnityEngine;
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

    [Header("UI That CAN Start The Game")]
    public List<GameObject> allowedTapUI; // 👈 whitelist

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
                // 🚫 Block unwanted UI taps
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
    // CHECK UI INPUT WITH WHITELIST
    // ==============================
    bool IsPointerOverBlockedUI()
    {
        if (EventSystem.current == null)
            return false;

        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (RaycastResult result in results)
        {
            GameObject hit = result.gameObject;

            // If it's in the allowed list → do NOT block
            foreach (GameObject allowed in allowedTapUI)
            {
                if (hit == allowed || hit.transform.IsChildOf(allowed.transform))
                    return false;
            }

            // Otherwise it's UI → block
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