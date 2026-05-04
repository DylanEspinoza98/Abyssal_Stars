using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Prefabs por Frecuencia")]
    [SerializeField] private EnemyBase _lowFreqEnemyPrefab;
    [SerializeField] private EnemyBase _midFreqEnemyPrefab;  
    [SerializeField] private EnemyBase _highFreqEnemyPrefab; 
    [SerializeField] private EnemyBase _subLowFreqEnemyPrefab;

    [Header("Puntos de Aparición")]
    [SerializeField] private Transform _lowSpawnPoint;
    [SerializeField] private Transform _midSpawnPoint;
    [SerializeField] private Transform _highSpawnPoint;
    [SerializeField] private Transform _subLowSpawnPoint;
    [SerializeField] private float _spawnRangeX = 2f;

    [Header("Intervalos de Seguridad (Timers)")]
    [SerializeField] private float _lowSpawnInterval = 0.5f;
    [SerializeField] private float _midSpawnInterval = 2.0f;
    [SerializeField] private float _highSpawnInterval = 1.0f;
    [SerializeField] private float _subLowSpawnInterval = 5.0f;

    private float _lastLowTime = -99f;
    private float _lastMidTime = -99f;
    private float _lastHighTime = -99f;
    private float _lastSubLowTime = -99f;

    [Header("Fase del Jefe")]
    [SerializeField] private float _timeToBoss = 60f;
    [SerializeField] private float _warningDuration = 3f;
    [SerializeField] private GameObject _bossPrefab;
    [SerializeField] private Transform _bossSpawnPoint;

    private float _levelTimer = 0f;
    private bool _bossSpawned = false;
    private bool _warningTriggered = false;
    private bool _isSpawningPaused = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Dentro del GameManager.cs
    private void Start()
    {
        if (AudioBeatDetector.Instance != null)
        {
            AudioBeatDetector.Instance.OnLowBeat += HandleLowFreqBeat;
            AudioBeatDetector.Instance.OnMidBeat += HandleMidFreqBeat;
            AudioBeatDetector.Instance.OnHighBeat += HandleHighFreqBeat;
            AudioBeatDetector.Instance.OnSubLowBeat += HandleSubLowFreqBeat;
        }
    }

    private void OnDestroy()
    {
        if (AudioBeatDetector.Instance != null)
        {
            AudioBeatDetector.Instance.OnLowBeat += HandleLowFreqBeat;
            AudioBeatDetector.Instance.OnMidBeat += HandleMidFreqBeat;
            AudioBeatDetector.Instance.OnHighBeat += HandleHighFreqBeat;
            AudioBeatDetector.Instance.OnSubLowBeat += HandleSubLowFreqBeat;
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
            _isSpawningPaused = true;

            if (BossWarningUI.Instance != null)
            {
                BossWarningUI.Instance.ShowWarning(_warningDuration);
            }
        }
    }

    private void HandleLowFreqBeat()
    {
        if (_isSpawningPaused) return;

        if (Time.time - _lastLowTime > _lowSpawnInterval)
        {
            SpawnEnemy(_lowFreqEnemyPrefab, _lowSpawnPoint);
            _lastLowTime = Time.time;
        }
    }

    private void HandleMidFreqBeat()
    {
        if (_isSpawningPaused) return;

        if (Time.time - _lastMidTime > _midSpawnInterval)
        {
            SpawnEnemy(_midFreqEnemyPrefab, _midSpawnPoint);
            _lastMidTime = Time.time;
        }
    }

    private void HandleHighFreqBeat()
    {
        if (_isSpawningPaused) return;

        if (Time.time - _lastHighTime > _highSpawnInterval)
        {
            SpawnEnemy(_highFreqEnemyPrefab, _highSpawnPoint);
            _lastHighTime = Time.time;
        }
    }

    private void HandleSubLowFreqBeat()
    {
        if (_isSpawningPaused) return;

        if (Time.time - _lastSubLowTime > _subLowSpawnInterval)
        {
            SpawnEnemy(_subLowFreqEnemyPrefab, _subLowSpawnPoint);
            _lastSubLowTime = Time.time;
        }
    }


    private void SpawnEnemy(EnemyBase prefab, Transform specificSpawnPoint)
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