using UnityEngine;
using System.Collections.Generic;

public class PlatformManager : MonoBehaviour
{
    [Header("Platform Settings")]
    public float platformWidth = 40f;
    public int platformsAlive = 4;
    public float despawnX = -120f;

    [Header("Spawn Offset")]
    public float firstSpawnOffset = 80f;

    [Header("Speed Settings")]
    public float startSpeed = 10f;
    public float maxSpeed = 16f;
    public float timeToMaxSpeed = 120f; // seconds (2 minutes)

    [Header("Sectors")]
    public List<GameObject> sector1;
    public List<GameObject> sector2;
    public List<GameObject> sector3;
    public List<GameObject> sector4;
    public List<GameObject> sector5;

    private List<GameObject> activePlatforms = new List<GameObject>();

    private float totalDistanceTravelled = 0f;
    private float gameplayTime = 0f;
    private bool gameStarted = false;

    public bool GameStarted => gameStarted;
    public float CurrentSpeed { get; private set; }

    void Update()
    {
        if (!gameStarted) return;

        UpdateSpeed();
        TrackDistance();
        CheckDespawn();
    }

    public void StartGame()
    {
        if (gameStarted) return;
        gameStarted = true;

        for (int i = 0; i < platformsAlive; i++)
        {
            SpawnPlatform();
        }
    }

    void UpdateSpeed()
    {
        gameplayTime += Time.deltaTime;

        float t = Mathf.Clamp01(gameplayTime / timeToMaxSpeed);
        CurrentSpeed = Mathf.Lerp(startSpeed, maxSpeed, t);
    }

    void TrackDistance()
    {
        totalDistanceTravelled += CurrentSpeed * Time.deltaTime;
    }

    void CheckDespawn()
    {
        // Clean out any nulls first (destroyed platforms, scene reload remnants)
        activePlatforms.RemoveAll(p => p == null);

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
        if (prefab == null) return;

        float spawnX;

        if (activePlatforms.Count == 0)
        {
            spawnX = firstSpawnOffset;
        }
        else
        {
            // Find the rightmost non-null platform to spawn after
            GameObject lastPlatform = null;
            for (int i = activePlatforms.Count - 1; i >= 0; i--)
            {
                if (activePlatforms[i] != null)
                {
                    lastPlatform = activePlatforms[i];
                    break;
                }
            }

            spawnX = lastPlatform != null
                ? lastPlatform.transform.position.x + platformWidth
                : firstSpawnOffset;
        }

        GameObject platform = Instantiate(prefab, new Vector3(spawnX, 0f, 0f), Quaternion.identity);

        PlatformMover mover = platform.GetComponent<PlatformMover>();
        if (mover != null)
            mover.Initialize(this);
        else
            Debug.LogError($"PlatformMover missing on prefab: {prefab.name}");

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

        if (currentSector == null || currentSector.Count == 0)
        {
            Debug.LogError($"Sector {sectorIndex + 1} is empty!");
            return null;
        }

        return currentSector[Random.Range(0, currentSector.Count)];
    }
}