using UnityEngine;
using System.Collections;

public class PowerUpSpawner : MonoBehaviour
{
    public static PowerUpSpawner Instance { get; private set; }

    [Header("Power Ups")]
    [SerializeField] private PowerUpLife _lifePowerUpPrefab;
    [SerializeField] private PowerUpShotgun _shotgunPowerUpPrefab;
    [SerializeField] private PowerUpFamiliar _familiarPowerUpPrefab;

    [Header("Spawn Settings")]
    [SerializeField] private float _minInterval = 8f;
    [SerializeField] private float _maxInterval = 18f;

    // Cuantas unidades por encima del borde superior de la camara aparece el power up
    [SerializeField] private float _spawnOffsetAboveCamera = 1.5f;

    [SerializeField] private float _minX = -3.5f;
    [SerializeField] private float _maxX = 3.5f;

    [Header("Probabilidades (deben sumar 1)")]
    // Ej: vida=0.3, shotgun=0.4, familiar=0.3
    [SerializeField] private float _lifeProbability = 0.3f;
    [SerializeField] private float _shotgunProbability = 0.4f;
    // El familiar toma el resto: 1 - vida - shotgun
    // No hace falta serializar el familiar, se calcula solo

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
        float camHalfHeight = _cam.orthographicSize;
        float spawnY = _cam.transform.position.y + camHalfHeight + _spawnOffsetAboveCamera;
        float randomX = Random.Range(_minX, _maxX);
        return new Vector3(randomX, spawnY, 0f);
    }

    private void SpawnRandom()
    {
        Vector3 spawnPos = GetSpawnPosition();
        float roll = Random.value;

        if (roll < _lifeProbability)
        {
            if (_lifePowerUpPrefab != null)
                Instantiate(_lifePowerUpPrefab, spawnPos, Quaternion.identity);
        }
        else if (roll < _lifeProbability + _shotgunProbability)
        {
            if (_shotgunPowerUpPrefab != null)
                Instantiate(_shotgunPowerUpPrefab, spawnPos, Quaternion.identity);
        }
        else
        {
            if (_familiarPowerUpPrefab != null)
                Instantiate(_familiarPowerUpPrefab, spawnPos, Quaternion.identity);
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

    public void SpawnFamiliar()
    {
        if (_familiarPowerUpPrefab != null)
            Instantiate(_familiarPowerUpPrefab, GetSpawnPosition(), Quaternion.identity);
    }
}