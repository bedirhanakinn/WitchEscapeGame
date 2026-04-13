using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameStart : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 10f;
    public float despawnX = -50f;

    [Header("Start Delay")]
    public float startDelay = 1f;

    [Header("Objects To Hide On Start")]
    public List<GameObject> objectsToHideOnStart;

    [Header("Objects To Disable On Tap")]
    public List<GameObject> objectsToDisable;

    [Header("Objects To Enable On Tap")]
    public List<GameObject> objectsToEnable;

    [Header("Objects To Enable After Movement Starts")]
    public List<GameObject> objectsToEnableAfterDelay;

    private bool gameStarted = false;
    private bool canMove = false;

    void Start()
    {
        foreach (GameObject obj in objectsToHideOnStart)
        {
            if (obj != null)
                obj.SetActive(false);
        }
    }

    void Update()
    {
        if (!gameStarted)
        {
            if (Input.GetMouseButtonDown(0))
            {
                StartGame();
            }
        }
        else if (canMove)
        {
            Move();
        }
    }

    void StartGame()
    {
        gameStarted = true;

        foreach (GameObject obj in objectsToDisable)
        {
            if (obj != null)
                obj.SetActive(false);
        }

        foreach (GameObject obj in objectsToEnable)
        {
            if (obj != null)
                obj.SetActive(true);
        }

        StartCoroutine(StartAfterDelay());
    }

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

    void Move()
    {
        transform.position += Vector3.left * moveSpeed * Time.deltaTime;

        if (transform.position.x <= despawnX)
        {
            Destroy(gameObject);
        }
    }
}