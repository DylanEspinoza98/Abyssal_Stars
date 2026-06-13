using UnityEngine;
using System;
using System.Collections;

public class AudioBeatDetector : MonoBehaviour
{
    [Header("Reproductores (Asignar en Inspector)")]
    [SerializeField] private AudioSource _levelAudioSource;
    [SerializeField] private AudioSource _bossAudioSource;

    private AudioSource _currentAudioSource;

    [Header("Configuración de Audio")]
    [SerializeField] private int _sampleSize = 1024;

    [Header("Frecuencias Bajas (Low)")]
    [SerializeField] private int _lowStartSample = 0;
    [SerializeField] private int _lowEndSample = 10;
    [SerializeField] private float _lowThreshold = 0.15f;
    [SerializeField] private float _lowCooldown = 0.15f;

    [Header("Frecuencias Medias (Mid)")]
    [SerializeField] private int _midStartSample = 11;
    [SerializeField] private int _midEndSample = 40;
    [SerializeField] private float _midThreshold = 0.08f;
    [SerializeField] private float _midCooldown = 0.30f;

    [Header("Frecuencias Altas (High)")]
    [SerializeField] private int _highStartSample = 41;
    [SerializeField] private int _highEndSample = 100;
    [SerializeField] private float _highThreshold = 0.05f;
    [SerializeField] private float _highCooldown = 5f;

    [Header("Frecuencias Sub-Bajas (Sub-Low)")]
    [SerializeField] private int _subLowStartSample = 101;
    [SerializeField] private int _subLowEndSample = 250;
    [SerializeField] private float _subLowThreshold = 0.06f;
    [SerializeField] private float _subLowCooldown = 2.0f;

    public static AudioBeatDetector Instance { get; private set; }

    public event Action OnLowBeat;
    public event Action OnMidBeat;
    public event Action OnHighBeat;
    public event Action OnSubLowBeat;

    private float[] _samples;
    private float _prevLowIntensity, _prevMidIntensity, _prevHighIntensity, _prevSubLowIntensity;
    private float _lastLowTime, _lastMidTime, _lastHighTime, _lastSubLowTime;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        _samples = new float[_sampleSize];

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopMusic();
        }

        _currentAudioSource = _levelAudioSource;
        if (_levelAudioSource != null) _levelAudioSource.Play();

        float t = Time.time;
        _lastLowTime = t; _lastMidTime = t; _lastHighTime = t; _lastSubLowTime = t;
    }

    void Update()
    {
        if (_currentAudioSource == null || !_currentAudioSource.isPlaying) return;

        _currentAudioSource.GetSpectrumData(_samples, 0, FFTWindow.BlackmanHarris);

        DetectLowBeats();
        DetectMidBeats();
        DetectHighBeats();
        DetectSubLowBeats();
    }

    void DetectLowBeats()
    {
        float intensity = SumSamples(_lowStartSample, _lowEndSample);
        if (intensity > _prevLowIntensity && intensity > _lowThreshold && Time.time - _lastLowTime > _lowCooldown)
        {
            OnLowBeat?.Invoke();
            _lastLowTime = Time.time;
        }
        _prevLowIntensity = intensity;
    }

    void DetectMidBeats()
    {
        float intensity = SumSamples(_midStartSample, _midEndSample);
        if (intensity > _prevMidIntensity && intensity > _midThreshold && Time.time - _lastMidTime > _midCooldown)
        {
            OnMidBeat?.Invoke();
            _lastMidTime = Time.time;
        }
        _prevMidIntensity = intensity;
    }

    void DetectHighBeats()
    {
        float intensity = SumSamples(_highStartSample, _highEndSample);
        if (intensity > _prevHighIntensity && intensity > _highThreshold && Time.time - _lastHighTime > _highCooldown)
        {
            OnHighBeat?.Invoke();
            _lastHighTime = Time.time;
        }
        _prevHighIntensity = intensity;
    }

    void DetectSubLowBeats()
    {
        float intensity = SumSamples(_subLowStartSample, _subLowEndSample);
        if (intensity > _prevSubLowIntensity && intensity > _subLowThreshold && Time.time - _lastSubLowTime > _subLowCooldown)
        {
            OnSubLowBeat?.Invoke();
            _lastSubLowTime = Time.time;
        }
        _prevSubLowIntensity = intensity;
    }

    float SumSamples(int from, int to)
    {
        float sum = 0f;
        int end = Mathf.Min(to, _samples.Length - 1);

        for (int i = from; i <= end; i++)
        {
            sum += Mathf.Abs(_samples[i]);
        }
        return sum;
    }
    public void SwapToBossMusic(float fadeDuration = 2f)
    {
        if (_bossAudioSource == null || _levelAudioSource == null) return;
        StartCoroutine(CrossfadeRoutine(_levelAudioSource, _bossAudioSource, fadeDuration));
    }

    private IEnumerator CrossfadeRoutine(AudioSource fadeOutSource, AudioSource fadeInSource, float duration)
    {
        fadeInSource.volume = 0f;
        fadeInSource.Play();

        _currentAudioSource = fadeInSource;

        float timer = 0f;
        float startVolumeOut = fadeOutSource.volume;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = timer / duration;

            fadeOutSource.volume = Mathf.Lerp(startVolumeOut, 0f, t);
            fadeInSource.volume = Mathf.Lerp(0f, 1f, t);

            yield return null;
        }

        fadeOutSource.Stop();
        fadeOutSource.volume = startVolumeOut;
        fadeInSource.volume = 1f;
    }

    public void StopMusic()
    {
        if (_levelAudioSource != null) _levelAudioSource.Stop();
        if (_bossAudioSource != null) _bossAudioSource.Stop();
    }
}