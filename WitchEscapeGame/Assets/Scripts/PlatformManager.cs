using UnityEngine;
using System.Collections.Generic;

public class PlatformManager : MonoBehaviour
{
    [Header("Platform Settings")]
    public float platformWidth = 40f;
    public int platformsAlive = 4;
    public float despawnX = -120f;

    [Header("Speed Settings")]
    public float startSpeed = 10f;
    public AnimationCurve speedCurve;
    public float speedCurveMultiplier = 0.05f;

    [Header("Sectors")]
    public List<GameObject> sector1;
    public List<GameObject> sector2;
    public List<GameObject> sector3;
    public List<GameObject> sector4;
    public List<GameObject> sector5;

    private List<GameObject> activePlatforms = new List<GameObject>();

    private float totalDistanceTravelled = 0f;
    private bool gameStarted = false;

    public bool GameStarted => gameStarted;
    public float CurrentSpeed { get; private set; }

    void Update()
    {
        if (!gameStarted)
        {
            if (Input.GetMouseButtonDown(0))
            {
                StartGame();
            }
            return;
        }

        UpdateSpeed();
        TrackDistance();
        CheckDespawn();
    }

    void StartGame()
    {
        gameStarted = true;

        for (int i = 0; i < platformsAlive; i++)
        {
            SpawnPlatform();
        }
    }

    void UpdateSpeed()
    {
        CurrentSpeed = startSpeed + speedCurve.Evaluate(totalDistanceTravelled * speedCurveMultiplier);
    }

    void TrackDistance()
    {
        totalDistanceTravelled += CurrentSpeed * Time.deltaTime;
    }

    void CheckDespawn()
    {
        if (activePlatforms.Count == 0) return;

        GameObject firstPlatform = activePlatforms[0];

        if (firstPlatform.transform.position.x <= despawnX)
        {
            activePlatforms.RemoveAt(0);
            Destroy(firstPlatform);
            SpawnPlatform();
        }
    }

    void SpawnPlatform()
    {
        GameObject prefab = GetPrefabFromCurrentSector();

        float spawnX;

        if (activePlatforms.Count == 0)
        {
            spawnX = 0f;
        }
        else
        {
            GameObject lastPlatform = activePlatforms[activePlatforms.Count - 1];
            spawnX = lastPlatform.transform.position.x + platformWidth;
        }

        GameObject platform = Instantiate(prefab, new Vector3(spawnX, 0f, 0f), Quaternion.identity);

        PlatformMover mover = platform.GetComponent<PlatformMover>();
        mover.Initialize(this);

        activePlatforms.Add(platform);
    }

    GameObject GetPrefabFromCurrentSector()
    {
        int sectorIndex = Mathf.FloorToInt(totalDistanceTravelled / 400f) % 5;

        List<GameObject> currentSector = sectorIndex switch
        {
            0 => sector1,
            1 => sector2,
            2 => sector3,
            3 => sector4,
            _ => sector5,
        };

        if (currentSector.Count == 0)
        {
            Debug.LogError("Sector is empty!");
            return null;
        }

        int randomIndex = Random.Range(0, currentSector.Count);
        return currentSector[randomIndex];
    }
}