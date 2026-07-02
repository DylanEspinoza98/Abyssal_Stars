using UnityEngine;
using System.Collections;

public class PowerUpSpawner : MonoBehaviour
{
    public static PowerUpSpawner Instance { get; private set; }

    [Header("Power Ups")]
    [SerializeField] private PowerUpLife _lifePowerUpPrefab;
    [SerializeField] private PowerUpShotgun _shotgunPowerUpPrefab;
    [SerializeField] private PowerUpFamiliar _familiarPowerUpPrefab;

    [Header("Tiempos de Spawn")]
    [SerializeField] private float _minInterval = 8f;
    [SerializeField] private float _maxInterval = 18f;

    [Header("Zona de Spawn (Idéntico al Fondo)")]
    [Tooltip("La altura exacta donde nacerán (Ej: 6 igual que tu BackgroundSpawner)")]
    [SerializeField] private float _spawnTopY = 6f;
    [SerializeField] private float _minX = -3.5f;
    [SerializeField] private float _maxX = 3.5f;

    [Header("Probabilidades (deben sumar 1)")]
    [SerializeField] private float _lifeProbability = 0.3f;
    [SerializeField] private float _shotgunProbability = 0.4f;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
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
        float randomX = Random.Range(_minX, _maxX);
        return new Vector3(randomX, _spawnTopY, 0f);
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

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(new Vector3(_minX, _spawnTopY, 0f), new Vector3(_maxX, _spawnTopY, 0f));

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(new Vector3(_minX, _spawnTopY + 0.5f, 0f), new Vector3(_minX, _spawnTopY - 0.5f, 0f));
        Gizmos.DrawLine(new Vector3(_maxX, _spawnTopY + 0.5f, 0f), new Vector3(_maxX, _spawnTopY - 0.5f, 0f));

        UnityEditor.Handles.Label(new Vector3(_minX, _spawnTopY + 0.2f, 0f), " Zona Spawn PowerUps");
    }
#endif
}