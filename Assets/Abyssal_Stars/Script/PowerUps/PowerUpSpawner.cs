using UnityEngine;
using System.Collections;

public class PowerUpSpawner : MonoBehaviour
{
    public static PowerUpSpawner Instance { get; private set; }

    [Header("Power Ups")]
    [SerializeField] private PowerUpLife _lifePowerUpPrefab;
    [SerializeField] private PowerUpShotgun _shotgunPowerUpPrefab;

    [Header("Spawn Settings")]
    [SerializeField] private float _minInterval = 8f;
    [SerializeField] private float _maxInterval = 18f;

    // Cuantas unidades por encima del borde superior de la camara aparece el power up
    [SerializeField] private float _spawnOffsetAboveCamera = 1.5f;

    [SerializeField] private float _minX = -3.5f;
    [SerializeField] private float _maxX = 3.5f;

    // Probabilidad 0-1: 0.4 = 40% vida, 60% shotgun
    [SerializeField] private float _lifeProbability = 0.4f;

    private Camera _cam;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        _cam = Camera.main;
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            float waitTime = Random.Range(_minInterval, _maxInterval);
            yield return new WaitForSeconds(waitTime);
            SpawnRandom();
        }
    }

    private Vector3 GetSpawnPosition()
    {
        // Calcula el borde superior de la camara en coordenadas de mundo
        float camHalfHeight = _cam.orthographicSize;
        float spawnY = _cam.transform.position.y + camHalfHeight + _spawnOffsetAboveCamera;
        float randomX = Random.Range(_minX, _maxX);
        return new Vector3(randomX, spawnY, 0f);
    }

    private void SpawnRandom()
    {
        Vector3 spawnPos = GetSpawnPosition();

        if (Random.value < _lifeProbability)
        {
            if (_lifePowerUpPrefab != null)
                Instantiate(_lifePowerUpPrefab, spawnPos, Quaternion.identity);
        }
        else
        {
            if (_shotgunPowerUpPrefab != null)
                Instantiate(_shotgunPowerUpPrefab, spawnPos, Quaternion.identity);
        }
    }

    public void SpawnLife()
    {
        if (_lifePowerUpPrefab != null)
            Instantiate(_lifePowerUpPrefab, GetSpawnPosition(), Quaternion.identity);
    }

    public void SpawnShotgun()
    {
        if (_shotgunPowerUpPrefab != null)
            Instantiate(_shotgunPowerUpPrefab, GetSpawnPosition(), Quaternion.identity);
    }
}