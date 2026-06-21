using UnityEngine;

public class AutoSFX : MonoBehaviour
{
    [SerializeField] private AudioClip sound;

    [SerializeField] private float volume = 1f;

    [SerializeField] private float pitchMin = 1f;

    [SerializeField] private float pitchMax = 1f;

    private void OnEnable()
    {
        if (SoundManager.Instance == null || sound == null)
            return;

        float pitch = Random.Range(pitchMin, pitchMax);

        SoundManager.Instance.PlaySFX(sound, volume, pitch);
    }
}