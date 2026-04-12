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

    [Header("Prefabs de Enemigos")]
    [SerializeField] private Kamikaze _kamikazePrefab;
    [SerializeField] private CircularEnemy _circularEnemyPrefab;

    [Header("Fase del Jefe")]
    [SerializeField] private float _timeToBoss = 60f; 
    [SerializeField] private GameObject _bossPrefab;
    [SerializeField] private Transform _bossSpawnPoint;
    private bool _bossSpawned = false;
    private float _levelTimer = 0f;
   

    [Header("Spawn Points")]
    [SerializeField] private Transform[] _spawnPoints;
    [Header("Ajustes de Despliegue")]
    [SerializeField] private float _spawnRangeX = 2f;

    private float[] _samples;
    private float _prevBassIntensity;
    private float _lastBassTime;
    private float _prevMidIntensity;
    private float _lastMidTime;
    private float _lastOrbitadorTime = -999f;
    private int _beatCount = 0;

    void Start()
    {
        _samples = new float[_sampleSize];
        if (_audioSource == null) _audioSource = GetComponent<AudioSource>();
        _lastBassTime = Time.time;
        _lastMidTime = Time.time;
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

        _audioSource.GetSpectrumData(_samples, 0, FFTWindow.BlackmanHarris);

        DetectBass();
        DetectMids();
    }

    void SpawnBoss()
    {
        _bossSpawned = true;
        Debug.Log("<color=red><b>AVISO:</b> ¡EL JEFE ESTÁ ENTRANDO!</color>");

        if (_bossPrefab != null)
        {
            Vector3 spawnPos = (_bossSpawnPoint != null) ? _bossSpawnPoint.position : _spawnPoints[0].position;
            Instantiate(_bossPrefab, spawnPos, Quaternion.identity);
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
            SpawnEnemy(_kamikazePrefab);
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

    void TrySpawnOrbitador()
    {
        if (Time.time - _lastOrbitadorTime > _orbitadorMinInterval)
        {
            SpawnEnemy(_circularEnemyPrefab);
            _lastOrbitadorTime = Time.time;
        }
    }

    float SumSamples(int from, int to)
    {
        float sum = 0f;
        int end = Mathf.Min(to, _samples.Length - 1);
        for (int i = from; i <= end; i++) sum += _samples[i];
        return sum;
    }

    void SpawnEnemy<T>(T prefab) where T : EnemyBase
    {
        if (prefab == null || _spawnPoints.Length == 0) return;
        int randomIndex = Random.Range(0, _spawnPoints.Length);
        Vector3 basePos = _spawnPoints[randomIndex].position;
        float randomX = Random.Range(-_spawnRangeX, _spawnRangeX);
        Vector3 finalPos = new Vector3(basePos.x + randomX, basePos.y, basePos.z);

        EnemyPool.Instance.GetEnemy(prefab, finalPos, prefab.transform.rotation);
    }
}