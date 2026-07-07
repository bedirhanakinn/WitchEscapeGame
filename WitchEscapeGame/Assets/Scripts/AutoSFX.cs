using System.Collections;
using UnityEngine;

public class AutoSFX : MonoBehaviour
{
    [SerializeField] private AudioClip sound;

    [SerializeField] private float volume = 1f;

    [SerializeField] private float pitchMin = 1f;

    [SerializeField] private float pitchMax = 1f;

    private void OnEnable()
    {
        StartCoroutine(PlayWhenReady());
    }

    private IEnumerator PlayWhenReady()
    {
        // Wait one frame so SoundManager has time to initialize
        yield return null;

        if (SoundManager.Instance == null || sound == null)
            yield break;

        float pitch = Random.Range(pitchMin, pitchMax);

        SoundManager.Instance.PlaySFX(sound, volume, pitch);
    }
}