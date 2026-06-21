using UnityEngine;

public class ButtonSFX : MonoBehaviour
{
    [SerializeField] private AudioClip clickSound;

    [SerializeField] private float volume = 1f;

    [SerializeField] private float pitchMin = 1f;

    [SerializeField] private float pitchMax = 1f;

    public void PlayClick()
    {
        if (SoundManager.Instance == null || clickSound == null)
            return;

        float pitch = Random.Range(pitchMin, pitchMax);

        SoundManager.Instance.PlaySFX(clickSound, volume, pitch);
    }
}