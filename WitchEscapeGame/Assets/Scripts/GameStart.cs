using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameStart : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 10f;
    public float despawnX = -50f;

    [Header("Start Delay")]
    public float startDelay = 1f; // Time after tap before movement starts

    [Header("Objects To Disable On Start")]
    public List<GameObject> objectsToDisable;

    [Header("Objects To Enable On Start")]
    public List<GameObject> objectsToEnable;

    private bool gameStarted = false;
    private bool canMove = false;

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

        // Disable selected objects
        foreach (GameObject obj in objectsToDisable)
        {
            if (obj != null)
                obj.SetActive(false);
        }

        // Enable selected objects
        foreach (GameObject obj in objectsToEnable)
        {
            if (obj != null)
                obj.SetActive(true);
        }

        // Start delay before movement
        StartCoroutine(StartAfterDelay());
    }

    IEnumerator StartAfterDelay()
    {
        yield return new WaitForSeconds(startDelay);
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