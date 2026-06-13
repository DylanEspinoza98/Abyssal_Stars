using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Mixer")]
    [SerializeField] private AudioMixer _mixer;

    [Header("Fuentes de Audio")]
    [SerializeField] private AudioSource _musicSource;
    [SerializeField] private AudioSource _sfxSource;

    private const string MUSIC_PARAM = "MusicVolume";
    private const string SFX_PARAM = "SFXVolume";

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (_musicSource != null) _musicSource.volume = 1f;
            if (_sfxSource != null) _sfxSource.volume = 1f;

            StartCoroutine(WaitForDataManagerAndApply());
        }
        else Destroy(gameObject);
    }

    private IEnumerator WaitForDataManagerAndApply()
    {
        while (DataManager.Instance == null) yield return null;

        for (int i = 0; i < 5; i++)
        {
            ApplySavedVolumes();
            yield return null;
        }
    }

    public void ApplySavedVolumes()
    {
        if (DataManager.Instance == null)
        {
            Debug.LogWarning("AudioManager: DataManager no disponible aun, usando defaults.");
            return;
        }

        SettingsData s = DataManager.Instance.SaveData.settings;
        ApplyToMixer(MUSIC_PARAM, s.musicVolume);
        ApplyToMixer(SFX_PARAM, s.sfxVolume);
    }

    public void SetMusicVolume(float linearValue)
    {
        ApplyToMixer(MUSIC_PARAM, linearValue);
    }

    public void SetSFXVolume(float linearValue)
    {
        ApplyToMixer(SFX_PARAM, linearValue);
    }

    public void PlayMusic(AudioClip clip)
    {
        if (_musicSource == null || clip == null) return;
        if (_musicSource.clip == clip && _musicSource.isPlaying) return;

        _musicSource.clip = clip;
        _musicSource.loop = true;
        _musicSource.Play();

        StartCoroutine(ReapplyVolumesNextFrames());
    }

    private IEnumerator ReapplyVolumesNextFrames()
    {
        for (int i = 0; i < 5; i++)
        {
            ApplySavedVolumes();
            yield return null;
        }
    }

    public void StopMusic()
    {
        if (_musicSource == null) return;

        _musicSource.Stop();
        _musicSource.clip = null;
    }

    public void PlaySFX(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null) return;

        if (_sfxSource != null)
        {
            _sfxSource.PlayOneShot(clip, volumeScale);
        }
        else
        {
            Debug.LogWarning("AudioManager: SFX Source no asignado.");
        }
    }

    private void ApplyToMixer(string parameter, float linearValue)
    {
        float dB = Mathf.Log10(Mathf.Max(linearValue, 0.0001f)) * 20f;
        _mixer.SetFloat(parameter, dB);
    }
}