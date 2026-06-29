using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BackgroundLayer : MonoBehaviour
{
    [Header("1. Identidad y Prefabs")]
    public string layerName = "Layer";
    [SerializeField] private GameObject[] _prefabs;

    [Tooltip("Si TRUE, usa una cola inteligente para no repetir el mismo objeto seguido (Ideal para 2 o 3 Planetas).")]
    [SerializeField] private bool _isUniqueLayer = false;

    [Header("2. Control Principal")]
    [Tooltip("Llave general. Si está apagado, la capa se pausa por completo.")]
    [SerializeField] private bool _canSpawn = true;
    [SerializeField] private float _spawnRangeX = 6f;
    [SerializeField] private float _initialDelay = 0f;

    [Header("3. Modo de Generación")]
    [Tooltip("FALSE: Generación constante (Estrellas). \nTRUE: Generación por Fases (Planetas/Meteoritos).")]
    [SerializeField] private bool _usePhases = false;

    [Header("-> Ajustes: Modo Continuo (Si _usePhases es FALSE)")]
    [SerializeField] private float _minSpawnTime = 0.2f;
    [SerializeField] private float _maxSpawnTime = 1f;

    [Header("-> Ajustes: Modo Fases (Si _usePhases es TRUE)")]
    [Tooltip("Tiempo de espera en silencio antes de que empiece el evento.")]
    [SerializeField] private float _minTimeBetweenPhases = 15f;
    [SerializeField] private float _maxTimeBetweenPhases = 30f;

    [Tooltip("Cuánto dura la lluvia de meteoritos o el evento.")]
    [SerializeField] private float _minPhaseDuration = 3f;
    [SerializeField] private float _maxPhaseDuration = 7f;

    [Tooltip("Qué tan rápido salen los objetos DURANTE la fase (Menos = Más rápido).")]
    [SerializeField] private float _spawnRateDuringPhase = 0.4f;

    [Header("4. Parallax y Escala")]
    [SerializeField] private float _minScale = 0.2f;
    [SerializeField] private float _maxScale = 0.8f;
    [SerializeField] private float _baseSpeed = 0.5f;

    [Header("5. Estética Visual")]
    [SerializeField] private int _sortingOrder = -10;
    [SerializeField] private bool _enableGlow = false;
    [SerializeField] private float _glowIntensity = 0.15f;
    [SerializeField][Range(0.5f, 4f)] private float _glowSpeed = 1.2f;
    [SerializeField] private float _scaleBreath = 0.02f;

    [Header("6. Memoria (Pool)")]
    [SerializeField] private int _poolInitialSize = 4;

    // Variables Privadas
    private float _speedMultiplier = 1f;
    private GameObject _lastSpawnedUnique;
    private float _killY = -12f;
    private Dictionary<GameObject, DecorPool> _pools = new Dictionary<GameObject, DecorPool>();
    private Queue<GameObject> _uniqueQueue = new Queue<GameObject>();
    private List<SpaceDecor> _activeDecors = new List<SpaceDecor>();

    private void Awake()
    {
        BackgroundSpawner spawner = GetComponentInParent<BackgroundSpawner>();
        if (spawner != null)
            _killY = spawner.KillY;

        InitPools();
        if (_isUniqueLayer) ReshuffleUniques();
    }

    private void OnEnable()
    {
        StartCoroutine(MainSpawnRoutine());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    public void SpawnAt(float yPosition, bool forceCommon = false)
    {
        float x = transform.position.x + Random.Range(-_spawnRangeX, _spawnRangeX);
        Vector3 pos = new Vector3(x, yPosition, 0f);
        SpawnObject(pos, forceCommon);
    }

    public void SetKillY(float killY) => _killY = killY;
    public void SetSpawning(bool state) => _canSpawn = state;


    public void SetSpeedMultiplier(float multiplier)
    {
        _speedMultiplier = multiplier;

        // Propagar inmediatamente a los objetos ya en pantalla
        foreach (SpaceDecor decor in _activeDecors)
            decor?.SetMultiplier(_speedMultiplier);
    }

    public void ReturnAll()
    {
        for (int i = _activeDecors.Count - 1; i >= 0; i--)
        {
            SpaceDecor decor = _activeDecors[i];
            if (decor == null) continue;

            ReturnToPool(decor.gameObject);
        }
        _activeDecors.Clear();
    }

    // Rutina Principal Unificada
    private IEnumerator MainSpawnRoutine()
    {
        yield return new WaitForSeconds(_initialDelay);

        while (true)
        {
            if (!_canSpawn)
            {
                yield return null;
                continue;
            }

            if (!_usePhases)
            {
                // MODO CONTINUO (Estrellas / Humo)
                SpawnAt(transform.position.y);
                yield return new WaitForSeconds(Random.Range(_minSpawnTime, _maxSpawnTime));
            }
            else
            {
                // MODO FASES (Lluvia de Meteoros / Planetas)

                //Fase de Silencio
                float waitTime = Random.Range(_minTimeBetweenPhases, _maxTimeBetweenPhases);
                yield return new WaitForSeconds(waitTime);

                //Fase Activa (Evento / Lluvia)
                float phaseDuration = Random.Range(_minPhaseDuration, _maxPhaseDuration);
                float elapsed = 0f;

                while (elapsed < phaseDuration && _canSpawn)
                {
                    float yRuido = transform.position.y + Random.Range(-1f, 2f);
                    SpawnAt(yRuido);

                    yield return new WaitForSeconds(_spawnRateDuringPhase);
                    elapsed += _spawnRateDuringPhase;
                }
            }
        }
    }

    // Lógica de Instanciación
    private void SpawnObject(Vector3 position, bool forceCommon = false)
    {
        GameObject prefab = PickPrefab(forceCommon);
        if (prefab == null || !_pools.ContainsKey(prefab)) return;

        DecorPool pool = _pools[prefab];
        Quaternion rot = Quaternion.Euler(0f, 0f, Random.Range(0f, 360f));
        GameObject obj = pool.Get(position, rot);

        // Escala y profundidad
        float scale = Random.Range(_minScale, _maxScale);
        obj.transform.localScale = Vector3.one * scale;

        float depthRatio = Mathf.Clamp01(Mathf.InverseLerp(_minScale, _maxScale, scale));
        float speed = Mathf.Clamp(_baseSpeed * Mathf.Lerp(0.1f, 1f, depthRatio), 0.05f, 99f);

        // SpriteRenderer
        float alpha = Mathf.Lerp(0.2f, 0.95f, depthRatio);
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sortingOrder = _sortingOrder;
            Color c = sr.color;
            c.a = alpha;
            sr.color = c;
        }

        if (_enableGlow)
        {
            DecorGlow glow = obj.GetComponent<DecorGlow>() ?? obj.AddComponent<DecorGlow>();
            glow.Setup(alpha, _glowIntensity, _glowSpeed, _scaleBreath);
        }

        // Movimiento
        SpaceDecor decor = obj.GetComponent<SpaceDecor>() ?? obj.AddComponent<SpaceDecor>();
        decor.Setup(speed, _killY);
        decor.SetMultiplier(_speedMultiplier);

        // Suscribirse al evento
        decor.OnOutOfBounds += HandleOutOfBounds;
        _activeDecors.Add(decor);
    }

    private void HandleOutOfBounds(SpaceDecor decor)
    {
        decor.OnOutOfBounds -= HandleOutOfBounds;
        _activeDecors.Remove(decor);

        DecorGlow glow = decor.GetComponent<DecorGlow>();
        glow?.ResetGlow();

        ReturnToPool(decor.gameObject);
    }

    // Pool y Memoria
    private void InitPools()
    {
        if (_prefabs == null) return;

        foreach (GameObject prefab in _prefabs)
        {
            if (prefab == null || _pools.ContainsKey(prefab)) continue;

            Transform poolParent = new GameObject($"Pool_{layerName}_{prefab.name}").transform;
            poolParent.SetParent(transform);
            _pools[prefab] = new DecorPool(prefab, poolParent, _poolInitialSize);
        }
    }

    private void ReturnToPool(GameObject obj)
    {
        foreach (var kv in _pools)
        {
            if (obj.name.StartsWith(kv.Key.name))
            {
                kv.Value.Return(obj);
                return;
            }
        }
        obj.SetActive(false);
    }

    // Filtro Anti-Repetición
    private GameObject PickPrefab(bool forceCommon)
    {
        if (_prefabs == null || _prefabs.Length == 0) return null;

        if (_isUniqueLayer && !forceCommon)
            return DequeueUnique();

        return _prefabs[Random.Range(0, _prefabs.Length)];
    }

    private GameObject DequeueUnique()
    {
        if (_uniqueQueue.Count == 0) ReshuffleUniques();

        GameObject popped = _uniqueQueue.Count > 0 ? _uniqueQueue.Dequeue() : null;
        _lastSpawnedUnique = popped;

        return popped;
    }

    private void ReshuffleUniques()
    {
        var list = new System.Collections.Generic.List<GameObject>(_prefabs);

        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }

        if (list.Count > 1 && _lastSpawnedUnique != null && list[0] == _lastSpawnedUnique)
        {
            int lastIndex = list.Count - 1;
            (list[0], list[lastIndex]) = (list[lastIndex], list[0]);
        }

        _uniqueQueue.Clear();
        foreach (var go in list) _uniqueQueue.Enqueue(go);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.4f, 1f, 0.6f, 0.35f);
        Gizmos.DrawWireCube(transform.position, new Vector3(_spawnRangeX * 2f, 1f, 0f));
    }
#endif
}