using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Music")]
    public AudioSource musicSource;
    public AudioClip backgroundMusic;

    [Header("SFX Pool")]
    [SerializeField] private int sfxPoolSize = 15;

    private AudioSource[] sfxSources;
    private int currentSourceIndex;

    [Header("Settings")]
    public bool musicEnabled = true;
    public bool sfxEnabled = true;

    private void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Load saved settings
        musicEnabled = PlayerPrefs.GetInt("MusicEnabled", 1) == 1;
        sfxEnabled = PlayerPrefs.GetInt("SFXEnabled", 1) == 1;

        CreateSFXPool();

        ApplySettings();
    }

    private void Start()
    {
        // Start background music
        if (backgroundMusic != null)
        {
            musicSource.clip = backgroundMusic;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    private void CreateSFXPool()
    {
        sfxSources = new AudioSource[sfxPoolSize];

        for (int i = 0; i < sfxPoolSize; i++)
        {
            GameObject sourceObject = new GameObject("SFXSource_" + i);
            sourceObject.transform.SetParent(transform);

            AudioSource source = sourceObject.AddComponent<AudioSource>();

            source.playOnAwake = false;
            source.loop = false;
            source.spatialBlend = 0f;

            sfxSources[i] = source;
        }
    }

    public void PlaySFX(AudioClip clip, float volume = 1f, float pitch = 1f)
    {
        if (!sfxEnabled || clip == null)
            return;

        AudioSource source = sfxSources[currentSourceIndex];

        source.pitch = pitch;
        source.PlayOneShot(clip, volume);

        currentSourceIndex++;

        if (currentSourceIndex >= sfxPoolSize)
        {
            currentSourceIndex = 0;
        }
    }

    public void ToggleMusic()
    {
        musicEnabled = !musicEnabled;

        PlayerPrefs.SetInt("MusicEnabled", musicEnabled ? 1 : 0);
        PlayerPrefs.Save();

        ApplySettings();
    }

    public void ToggleSFX()
    {
        sfxEnabled = !sfxEnabled;

        PlayerPrefs.SetInt("SFXEnabled", sfxEnabled ? 1 : 0);
        PlayerPrefs.Save();

        ApplySettings();
    }

    private void ApplySettings()
    {
        // Music
        musicSource.mute = !musicEnabled;

        // SFX
        foreach (AudioSource source in sfxSources)
        {
            source.mute = !sfxEnabled;
        }
    }

    public bool IsMusicEnabled()
    {
        return musicEnabled;
    }

    public bool IsSFXEnabled()
    {
        return sfxEnabled;
    }
}