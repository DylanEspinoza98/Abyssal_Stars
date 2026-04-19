using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Prefabs de Enemigos")]
    [SerializeField] private Kamikaze _kamikazePrefab;
    [SerializeField] private CircularEnemy _circularEnemyPrefab;
    [SerializeField] private EnemyGateKeeper _barreraPrefab;
    [SerializeField] private EnemyViking _vikingPrefab;

    [Header("Puntos de Aparición Específicos")]
    [SerializeField] private Transform _kamikazeSpawnPoint;
    [SerializeField] private Transform _orbitadorSpawnPoint;
    [SerializeField] private Transform _barreraSpawnPoint;
    [SerializeField] private Transform _vikingSpawnPoint;
    [SerializeField] private float _spawnRangeX = 2f;

    [Header("Lógica de Orbitador")]
    [SerializeField] private float _orbitadorMinInterval = 30f;
    private float _lastOrbitadorTime = -999f;

    [Header("Fase del Jefe")]
    [SerializeField] private float _timeToBoss = 60f;
    [SerializeField] private float _warningDuration = 3f;
    [SerializeField] private GameObject _bossPrefab;
    [SerializeField] private Transform _bossSpawnPoint;

    private int _beatCount = 0;
    private int _beatsPerMeasure = 4;
    private float _levelTimer = 0f;
    private bool _bossSpawned = false;
    private bool _warningTriggered = false;
    private bool _isSpawningPaused = false; // Para detener enemigos cuando llega el jefe

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Nos suscribimos a los avisos del AudioBeatDetector
        if (AudioBeatDetector.Instance != null)
        {
            AudioBeatDetector.Instance.OnBassBeat += HandleBassBeat;
            AudioBeatDetector.Instance.OnMidBeat += HandleMidBeat;
            AudioBeatDetector.Instance.OnHighBeat += HandleHighBeat;
            AudioBeatDetector.Instance.OnVikingBeat += HandleVikingBeat;
        }
    }

    private void OnDestroy()
    {
        // Limpieza de memoria vital
        if (AudioBeatDetector.Instance != null)
        {
            AudioBeatDetector.Instance.OnBassBeat -= HandleBassBeat;
            AudioBeatDetector.Instance.OnMidBeat -= HandleMidBeat;
            AudioBeatDetector.Instance.OnHighBeat -= HandleHighBeat;
            AudioBeatDetector.Instance.OnVikingBeat -= HandleVikingBeat;
        }
    }

    private void Update()
    {
        if (_bossSpawned) return;

        _levelTimer += Time.deltaTime;

        if (_levelTimer >= _timeToBoss && !_bossSpawned)
        {
            SpawnBoss();
            return;
        }

        if (_levelTimer >= _timeToBoss - _warningDuration && !_warningTriggered)
        {
            _warningTriggered = true;
            _isSpawningPaused = true; // Cortamos el flujo de enemigos normales

            if (BossWarningUI.Instance != null)
            {
                BossWarningUI.Instance.ShowWarning(_warningDuration);
            }
        }
    }

    // --- REACCIONES A LA MÚSICA ---
    private void HandleBassBeat()
    {
        if (_isSpawningPaused) return;

        _beatCount = (_beatCount % _beatsPerMeasure) + 1;
        SpawnEnemy(_kamikazePrefab, _kamikazeSpawnPoint);

        if (_beatCount == _beatsPerMeasure) TrySpawnOrbitador();
    }

    private void HandleMidBeat()
    {
        if (_isSpawningPaused) return;
        TrySpawnOrbitador();
    }

    private void HandleHighBeat()
    {
        if (_isSpawningPaused) return;
        SpawnEnemy(_barreraPrefab, _barreraSpawnPoint);
    }

    private void HandleVikingBeat()
    {
        if (_isSpawningPaused) return;
        SpawnEnemy(_vikingPrefab, _vikingSpawnPoint);
    }

    private void TrySpawnOrbitador()
    {
        if (Time.time - _lastOrbitadorTime > _orbitadorMinInterval)
        {
            SpawnEnemy(_circularEnemyPrefab, _orbitadorSpawnPoint);
            _lastOrbitadorTime = Time.time;
        }
    }

    private void SpawnEnemy<T>(T prefab, Transform specificSpawnPoint) where T : EnemyBase
    {
        if (prefab == null || specificSpawnPoint == null) return;

        Vector3 basePos = specificSpawnPoint.position;
        float randomX = Random.Range(-_spawnRangeX, _spawnRangeX);
        Vector3 finalPos = new Vector3(basePos.x + randomX, basePos.y, basePos.z);

        EnemyPool.Instance.GetEnemy(prefab, finalPos, prefab.transform.rotation);
    }

    private void SpawnBoss()
    {
        _bossSpawned = true;
        if (_bossPrefab != null && _bossSpawnPoint != null)
        {
            Instantiate(_bossPrefab, _bossSpawnPoint.position, Quaternion.identity);
        }
    }
}