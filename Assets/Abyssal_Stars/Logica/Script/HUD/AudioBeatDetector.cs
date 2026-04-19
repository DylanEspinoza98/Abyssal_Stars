using UnityEngine;
using System; // ¡CRÍTICO para usar los event Action!

public class AudioBeatDetector : MonoBehaviour
{
    [Header("Configuracion de Audio")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private int _sampleSize = 1024;

    [Header("Banda de Bajos - Kamikaze")]
    [SerializeField] private int _bassStartSample = 0;
    [SerializeField] private int _bassEndSample = 10;
    [SerializeField] private float _bassThreshold = 0.15f;
    [SerializeField] private float _bassCooldown = 0.15f;

    [Header("Banda de Medios - Orbitador")]
    [SerializeField] private int _midStartSample = 11;
    [SerializeField] private int _midEndSample = 40;
    [SerializeField] private float _midThreshold = 0.08f;
    [SerializeField] private float _midCooldown = 0.30f;

    [Header("Banda de Agudos - GateKeeper")]
    [SerializeField] private int _highStartSample = 41;
    [SerializeField] private int _highEndSample = 100;
    [SerializeField] private float _highThreshold = 0.05f;
    [SerializeField] private float _highCooldown = 5f;

    [Header("Banda de Medios-Altos - Viking")]
    [SerializeField] private int _vikingStartSample = 101;
    [SerializeField] private int _vikingEndSample = 250;
    [SerializeField] private float _vikingThreshold = 0.06f;
    [SerializeField] private float _vikingCooldown = 2.0f;

    public static AudioBeatDetector Instance { get; private set; }

    public event Action OnBassBeat;
    public event Action OnMidBeat;
    public event Action OnHighBeat;
    public event Action OnVikingBeat;

    private float[] _samples;
    private float _prevBassIntensity, _prevMidIntensity, _prevHighIntensity, _prevVikingIntensity;
    private float _lastBassTime, _lastMidTime, _lastHighTime, _lastVikingTime;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        _samples = new float[_sampleSize];
        if (_audioSource == null) _audioSource = GetComponent<AudioSource>();

        float t = Time.time;
        _lastBassTime = t; _lastMidTime = t; _lastHighTime = t; _lastVikingTime = t;
    }

    void Update()
    {
        if (_audioSource == null || !_audioSource.isPlaying) return;

        _audioSource.GetSpectrumData(_samples, 0, FFTWindow.BlackmanHarris);

        DetectBass();
        DetectMids();
        DetectHighs();
        DetectVikingBeats();
    }

    void DetectBass()
    {
        float intensity = SumSamples(_bassStartSample, _bassEndSample);
        if (intensity > _prevBassIntensity && intensity > _bassThreshold && Time.time - _lastBassTime > _bassCooldown)
        {
            OnBassBeat?.Invoke(); 
            _lastBassTime = Time.time;
        }
        _prevBassIntensity = intensity;
    }

    void DetectMids()
    {
        float intensity = SumSamples(_midStartSample, _midEndSample);
        if (intensity > _prevMidIntensity && intensity > _midThreshold && Time.time - _lastMidTime > _midCooldown)
        {
            OnMidBeat?.Invoke();
            _lastMidTime = Time.time;
        }
        _prevMidIntensity = intensity;
    }

    void DetectHighs()
    {
        float intensity = SumSamples(_highStartSample, _highEndSample);
        if (intensity > _prevHighIntensity && intensity > _highThreshold && Time.time - _lastHighTime > _highCooldown)
        {
            OnHighBeat?.Invoke();
            _lastHighTime = Time.time;
        }
        _prevHighIntensity = intensity;
    }

    void DetectVikingBeats()
    {
        float intensity = SumSamples(_vikingStartSample, _vikingEndSample);
        if (intensity > _prevVikingIntensity && intensity > _vikingThreshold && Time.time - _lastVikingTime > _vikingCooldown)
        {
            OnVikingBeat?.Invoke();
            _lastVikingTime = Time.time;
        }
        _prevVikingIntensity = intensity;
    }

    float SumSamples(int from, int to)
    {
        float sum = 0f;
        int end = Mathf.Min(to, _samples.Length - 1);
        for (int i = from; i <= end; i++) sum += Mathf.Abs(_samples[i]);
        return sum / (_audioSource.volume > 0 ? _audioSource.volume : 0.1f);
    }

    public void StopMusic()
    {
        if (_audioSource != null && _audioSource.isPlaying) _audioSource.Stop();
    }
}