using UnityEngine;

public class AudioBeatDetector : MonoBehaviour
{
    [Header("Configuracion de Audio")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private int _sampleSize = 1024;
    [SerializeField] private int _beatsPerMeasure = 4;

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
    [SerializeField] private float _orbitadorMinInterval = 30f;


    [Header("Banda de Agudos - GateKeeper")]
    [SerializeField] private int _highStartSample = 41;
    [SerializeField] private int _highEndSample = 100;
    [SerializeField] private float _highThreshold = 0.05f;
    [SerializeField] private float _highCooldown = 5f; 


    [Header("Banda de Medios-Altos - Vikingo")]
    [SerializeField] private int _vikingStartSample = 101;
    [SerializeField] private int _vikingEndSample = 250;
    [SerializeField] private float _vikingThreshold = 0.06f;
    [SerializeField] private float _vikingCooldown = 2.0f;

    [Header("Prefabs de Enemigos")]
    [SerializeField] private Kamikaze _kamikazePrefab;
    [SerializeField] private CircularEnemy _circularEnemyPrefab;
    [SerializeField] private EnemyGateKeeper _barreraPrefab;
    [SerializeField] private EnemyViking _vikingPrefab;

    [Header("Fase del Jefe")]
    [SerializeField] private float _timeToBoss = 60f;
    [SerializeField] private float _warningDuration = 3f;
    [SerializeField] private GameObject _bossPrefab;

    [Header("Puntos de Aparición Específicos")]
    [SerializeField] private Transform _kamikazeSpawnPoint;
    [SerializeField] private Transform _orbitadorSpawnPoint;
    [SerializeField] private Transform _barreraSpawnPoint; 
    [SerializeField] private Transform _bossSpawnPoint;
    [SerializeField] private Transform _vikingSpawnPoint;

    [Header("Ajustes de Despliegue")]
    [SerializeField] private float _spawnRangeX = 2f;

    public static AudioBeatDetector Instance { get; private set; }

    private float[] _samples;
    private float _prevBassIntensity;
    private float _lastBassTime;
    private float _prevMidIntensity;
    private float _lastMidTime;
    private float _prevHighIntensity;
    private float _lastHighTime;
    private float _lastOrbitadorTime = -999f;
    private int _beatCount = 0;
    private float _prevVikingIntensity;
    private float _lastVikingTime;

    private bool _bossSpawned = false;
    private bool _warningTriggered = false;
    private float _levelTimer = 0f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        _samples = new float[_sampleSize];
        if (_audioSource == null) _audioSource = GetComponent<AudioSource>();
        _lastBassTime = Time.time;
        _lastMidTime = Time.time;
        _lastHighTime = Time.time;
        _lastVikingTime = Time.time;
    }

    void Update()
    {
        if (_audioSource == null || !_audioSource.isPlaying) return;
        if (_bossSpawned) return;

        _levelTimer += Time.deltaTime;

        if (_levelTimer >= _timeToBoss)
        {
            SpawnBoss();
            return;
        }

        if (_levelTimer >= _timeToBoss - _warningDuration)
        {
            if (!_warningTriggered)
            {
                if (BossWarningUI.Instance != null)
                {
                    BossWarningUI.Instance.ShowWarning(_warningDuration);
                }
                _warningTriggered = true;
            }
            return;
        }

        _audioSource.GetSpectrumData(_samples, 0, FFTWindow.BlackmanHarris);
        DetectBass();
        DetectMids();
        DetectHighs();
        DetectVikingBeats();
    }

    void SpawnBoss()
    {
        _bossSpawned = true;
        if (_bossPrefab != null && _bossSpawnPoint != null)
        {
            Instantiate(_bossPrefab, _bossSpawnPoint.position, Quaternion.identity);
        }
    }

    void DetectBass()
    {
        float intensity = SumSamples(_bassStartSample, _bassEndSample);
        bool isPeak = intensity > _prevBassIntensity;
        bool overThresh = intensity > _bassThreshold;
        bool cooledDown = Time.time - _lastBassTime > _bassCooldown;

        if (isPeak && overThresh && cooledDown)
        {
            _beatCount = (_beatCount % _beatsPerMeasure) + 1;
            SpawnEnemy(_kamikazePrefab, _kamikazeSpawnPoint);
            if (_beatCount == _beatsPerMeasure) TrySpawnOrbitador();
            _lastBassTime = Time.time;
        }
        _prevBassIntensity = intensity;
    }

    void DetectMids()
    {
        float intensity = SumSamples(_midStartSample, _midEndSample);
        bool isPeak = intensity > _prevMidIntensity;
        bool overThresh = intensity > _midThreshold;
        bool cooledDown = Time.time - _lastMidTime > _midCooldown;

        if (isPeak && overThresh && cooledDown)
        {
            TrySpawnOrbitador();
            _lastMidTime = Time.time;
        }
        _prevMidIntensity = intensity;
    }

    void DetectHighs()
    {
        float intensity = SumSamples(_highStartSample, _highEndSample);
        bool isPeak = intensity > _prevHighIntensity;
        bool overThresh = intensity > _highThreshold;
        bool cooledDown = Time.time - _lastHighTime > _highCooldown;

        if (isPeak && overThresh && cooledDown)
        {
            SpawnEnemy(_barreraPrefab, _barreraSpawnPoint);
            _lastHighTime = Time.time;
        }
        _prevHighIntensity = intensity;
    }
    void DetectVikingBeats()
    {
        float intensity = SumSamples(_vikingStartSample, _vikingEndSample);
        bool isPeak = intensity > _prevVikingIntensity;
        bool overThresh = intensity > _vikingThreshold;
        bool cooledDown = Time.time - _lastVikingTime > _vikingCooldown;

        if (isPeak && overThresh && cooledDown)
        {
            SpawnEnemy(_vikingPrefab, _vikingSpawnPoint);
            _lastVikingTime = Time.time;
        }
        _prevVikingIntensity = intensity;
    }

    void TrySpawnOrbitador()
    {
        if (Time.time - _lastOrbitadorTime > _orbitadorMinInterval)
        {
            SpawnEnemy(_circularEnemyPrefab, _orbitadorSpawnPoint);
            _lastOrbitadorTime = Time.time;
        }
    }

    float SumSamples(int from, int to)
    {
        float sum = 0f;
        int end = Mathf.Min(to, _samples.Length - 1);
        for (int i = from; i <= end; i++)
        {
            sum += Mathf.Abs(_samples[i]);
        }
        return sum / (_audioSource.volume > 0 ? _audioSource.volume : 0.1f);
    }

    void SpawnEnemy<T>(T prefab, Transform specificSpawnPoint) where T : EnemyBase
    {
        if (prefab == null || specificSpawnPoint == null) return;

        Vector3 basePos = specificSpawnPoint.position;
        float randomX = Random.Range(-_spawnRangeX, _spawnRangeX);
        Vector3 finalPos = new Vector3(basePos.x + randomX, basePos.y, basePos.z);

        EnemyPool.Instance.GetEnemy(prefab, finalPos, prefab.transform.rotation);
    }
    public void StopMusic()
    {
        if (_audioSource != null && _audioSource.isPlaying)
        {
            _audioSource.Stop();
        }
    }
}