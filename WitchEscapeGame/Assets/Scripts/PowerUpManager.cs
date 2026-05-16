using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PowerUpManager : MonoBehaviour
{
    [Header("References")]
    public PlayerController playerController;

    [Header("Normal States")]
    public GameObject normalModel;
    public GameObject stumbleModel;
    public GameObject deathModel;

    [Header("Power Ups")]
    public List<PowerUpData> powerUps =
        new List<PowerUpData>();

    [Header("UI")]
    public PowerUpUI powerUI;

    private CollectibleType currentType;
    private int currentCount = 0;

    private bool hasCollectedAnything = false;

    private bool powerActive = false;

    private Queue<CollectibleType> queuedPowers =
        new Queue<CollectibleType>();

    private PowerUpData activePower;

    void Start()
    {
        DisableAllPowerModels();

        UpdateVisualState();
    }

    public void Collect(CollectibleType type)
    {
        // POWER QUEUE
        if (powerActive)
        {
            HandleCollectionWhilePowered(type);
            return;
        }

        HandleNormalCollection(type);
    }

    void HandleNormalCollection(CollectibleType type)
    {
        // FIRST PICKUP
        if (!hasCollectedAnything)
        {
            currentType = type;
            currentCount = 1;
            hasCollectedAnything = true;
        }
        else
        {
            // SAME TYPE
            if (currentType == type)
            {
                currentCount++;
            }
            // DIFFERENT TYPE
            else
            {
                currentType = type;
                currentCount = 1;
            }
        }

        powerUI.ShowCollect(
            GetPowerData(currentType).iconSprite,
            currentCount
        );

        // GOT 3
        if (currentCount >= 3)
        {
            ActivatePower(currentType);

            currentCount = 0;
            hasCollectedAnything = false;
        }
    }

    void HandleCollectionWhilePowered(
        CollectibleType type
    )
    {
        if (!hasCollectedAnything)
        {
            currentType = type;
            currentCount = 1;
            hasCollectedAnything = true;
        }
        else
        {
            if (currentType == type)
            {
                currentCount++;
            }
            else
            {
                currentType = type;
                currentCount = 1;
            }
        }

        powerUI.ShowCollect(
            GetPowerData(currentType).iconSprite,
            currentCount
        );

        if (currentCount >= 3)
        {
            queuedPowers.Enqueue(type);

            currentCount = 0;
            hasCollectedAnything = false;
        }
    }

    void ActivatePower(CollectibleType type)
    {
        PowerUpData data =
            GetPowerData(type);

        if (data == null)
            return;

        activePower = data;

        StartCoroutine(
            PowerRoutine(data)
        );
    }

    IEnumerator PowerRoutine(PowerUpData data)
    {
        powerActive = true;

        UpdateVisualState();

        yield return new WaitForSeconds(
            data.duration
        );

        powerActive = false;

        DisableAllPowerModels();

        UpdateVisualState();

        // CHECK QUEUE
        if (queuedPowers.Count > 0)
        {
            CollectibleType queued =
                queuedPowers.Dequeue();

            ActivatePower(queued);
        }
    }

    public void UpdateVisualState()
    {
        DisableEverything();

        // DEATH PRIORITY
        if (playerController.IsDead())
        {
            deathModel.SetActive(true);
            return;
        }

        // POWER PRIORITY
        if (powerActive && activePower != null)
        {
            activePower.powerModel.SetActive(true);
            return;
        }

        // STUMBLE
        if (playerController.IsStumbling())
        {
            stumbleModel.SetActive(true);
            return;
        }

        // NORMAL
        normalModel.SetActive(true);
    }

    void DisableEverything()
    {
        normalModel.SetActive(false);
        stumbleModel.SetActive(false);
        deathModel.SetActive(false);

        DisableAllPowerModels();
    }

    void DisableAllPowerModels()
    {
        foreach (PowerUpData p in powerUps)
        {
            if (p.powerModel != null)
            {
                p.powerModel.SetActive(false);
            }
        }
    }

    PowerUpData GetPowerData(
        CollectibleType type
    )
    {
        foreach (PowerUpData p in powerUps)
        {
            if (p.type == type)
                return p;
        }

        return null;
    }

    public bool IsPowerActive()
    {
        return powerActive;
    }
}