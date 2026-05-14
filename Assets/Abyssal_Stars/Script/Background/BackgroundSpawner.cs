using UnityEngine;
using System.Collections;

public class BackgroundSpawner : MonoBehaviour
{
    [Header("Prefabs Comunes (Nebulosas, Estrellas)")]
    [SerializeField] private GameObject[] _commonPrefabs;

    [Header("Prefabs Raros (Planetas, Estaciones)")]
    [SerializeField] private GameObject[] _rarePrefabs;
    [SerializeField][Range(0f, 1f)] private float _rareSpawnChance = 0.25f;

    [Header("Tiempos de Generación")]
    [SerializeField] private float _minSpawnTime = 6f;
    [SerializeField] private float _maxSpawnTime = 14f;
    [SerializeField] private float _killY = -10f;

    [Header("Límites de Dispersión (Relativos)")]
    [SerializeField] private float _spawnRangeX = 6f;
    [SerializeField] private float _spawnRangeY = 2.0f;

    [Header("Ajustes de Profundidad (Parallax)")]
    [SerializeField] private float _minScale = 0.2f;
    [SerializeField] private float _maxScale = 1.1f;
    [SerializeField] private float _baseSpeed = 1.2f;
    [SerializeField] private int _sortingOrder = -10;

    [Header("Pre-Generación al Iniciar (Pre-Warm)")]
    [SerializeField] private bool _preSpawnAtStart = true;
    [SerializeField] private int _preSpawnAmount = 3;

    private GameObject _lastRareSpawned;

    private void Start()
    {
        if (_preSpawnAtStart)
        {
            PreSpawnObjects();
        }

        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        yield return new WaitForSeconds(Random.Range(3f, 7f));

        while (true)
        {
            SpawnDecorObject(false);

            float waitTime = Random.Range(_minSpawnTime, _maxSpawnTime);
            yield return new WaitForSeconds(waitTime);
        }
    }

    private void PreSpawnObjects()
    {
        for (int i = 0; i < _preSpawnAmount; i++)
        {
            float lerpRatio = (float)i / _preSpawnAmount;
            float spawnY = Mathf.Lerp(transform.position.y, _killY + 2f, lerpRatio);

            Vector3 spawnPos = new Vector3(
                transform.position.x + Random.Range(-_spawnRangeX, _spawnRangeX),
                spawnY,
                0f
            );

            CreateObjectAt(spawnPos);
        }
    }

    private void SpawnDecorObject(bool preWarm)
    {
        float randomX = Random.Range(-_spawnRangeX, _spawnRangeX);
        float randomY = Random.Range(-_spawnRangeY, _spawnRangeY);
        Vector3 spawnPos = transform.position + new Vector3(randomX, randomY, 0f);

        CreateObjectAt(spawnPos);
    }

    private void CreateObjectAt(Vector3 position)
    {
        GameObject prefabToSpawn = DeterminarPrefab();

        if (prefabToSpawn == null) return;

        GameObject newObj = Instantiate(prefabToSpawn, position, Quaternion.Euler(0f, 0f, Random.Range(0f, 360f)));

        float randomScale = Random.Range(_minScale, _maxScale);
        newObj.transform.localScale = new Vector3(randomScale, randomScale, 1f);

        float speedRatio = (randomScale - _minScale) / (_maxScale - _minScale);
        speedRatio = Mathf.Clamp(speedRatio, 0.15f, 1f);
        float finalSpeed = _baseSpeed * speedRatio;

        SpriteRenderer sr = newObj.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingOrder = _sortingOrder;

            Color color = sr.color;
            color.a = Mathf.Lerp(0.35f, 0.9f, speedRatio);
            sr.color = color;
        }

        BackgroundObject bgComp = newObj.AddComponent<BackgroundObject>();
        bgComp.Setup(finalSpeed, _killY);
    }

    private GameObject DeterminarPrefab()
    {
        bool spawnRare = Random.value <= _rareSpawnChance && _rarePrefabs.Length > 0;

        if (spawnRare)
        {
            if (_rarePrefabs.Length == 1) return _rarePrefabs[0];

            GameObject selectedRare = _rarePrefabs[Random.Range(0, _rarePrefabs.Length)];

            int attempts = 5;
            while (selectedRare == _lastRareSpawned && attempts > 0)
            {
                selectedRare = _rarePrefabs[Random.Range(0, _rarePrefabs.Length)];
                attempts--;
            }

            _lastRareSpawned = selectedRare;
            return selectedRare;
        }
        else if (_commonPrefabs.Length > 0)
        {
            return _commonPrefabs[Random.Range(0, _commonPrefabs.Length)];
        }

        return null;
    }
}