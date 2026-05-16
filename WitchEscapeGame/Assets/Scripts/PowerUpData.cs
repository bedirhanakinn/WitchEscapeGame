using UnityEngine;

[System.Serializable]
public class PowerUpData
{
    public CollectibleType type;

    [Header("Visuals")]
    public GameObject powerModel;

    [Header("UI")]
    public Sprite iconSprite;

    [Header("Settings")]
    public float duration = 7f;
}