using UnityEngine;

/// <summary>
/// Detecta beats musicales en tiempo real y spawnea enemigos
/// sincronizados con el ritmo.
/// Kamikaze = bajos (frecuente)
/// Orbitador = medios (cada X segundos minimo)
/// </summary>
public class AudioBeatDetector : MonoBehaviour
{
    // ---------------------------------------------
    // CONFIGURACION DE AUDIO
    // ---------------------------------------------
    [Header("Configuracion de Audio")]
    [SerializeField] private AudioSource _audioSource;

    [Tooltip("Cuantas muestras FFT se analizan por frame. Potencia de 2.")]
    [SerializeField] private int _sampleSize = 1024;

    [Header("Compas Musical")]
    [Tooltip("Cantidad de beats por compas (4 = ritmo 4/4, 3 = vals, etc.)")]
    [SerializeField] private int _beatsPerMeasure = 4;

    // ---------------------------------------------
    // BANDA DE BAJOS — KAMIKAZE
    // ---------------------------------------------
    [Header("Banda de Bajos - Kamikaze")]
    [SerializeField] private int _bassStartSample = 0;
    [SerializeField] private int _bassEndSample = 10;
    [SerializeField] private float _bassThreshold = 0.15f;
    [SerializeField] private float _bassCooldown = 0.15f;

    // ---------------------------------------------
    // BANDA DE MEDIOS — ORBITADOR
    // ---------------------------------------------
    [Header("Banda de Medios - Orbitador")]
    [SerializeField] private int _midStartSample = 11;
    [SerializeField] private int _midEndSample = 40;
    [SerializeField] private float _midThreshold = 0.08f;
    [SerializeField] private float _midCooldown = 0.30f;

    [Tooltip("Tiempo minimo entre cada Orbitador sin importar cuantos picos haya en medios")]
    [SerializeField] private float _orbitadorMinInterval = 30f;

    // ---------------------------------------------
    // PREFABS DE ENEMIGOS
    // ---------------------------------------------
    [Header("Prefabs de Enemigos")]
    [SerializeField] private Kamikaze _kamikazePrefab;
    [SerializeField] private CircularEnemy _circularEnemyPrefab;

    // ---------------------------------------------
    // SPAWN POINTS
    // ---------------------------------------------
    [Header("Spawn Points")]
    [SerializeField] private Transform[] _spawnPoints;

    [Header("Ajustes de Despliegue")]
    [Tooltip("Dispersion horizontal aleatoria desde el spawn point")]
    [SerializeField] private float _spawnRangeX = 2f;

    // ---------------------------------------------
    // ESTADO INTERNO
    // ---------------------------------------------
    private float[] _samples;

    private float _prevBassIntensity;
    private float _lastBassTime;

    private float _prevMidIntensity;
    private float _lastMidTime;

    // -999 para que pueda spawnear al inicio de la cancion
    private float _lastOrbitadorTime = -999f;

    private int _beatCount = 0;

    // ---------------------------------------------
    // INICIALIZACION
    // ---------------------------------------------
    void Start()
    {
        _samples = new float[_sampleSize];

        if (_audioSource == null)
            _audioSource = GetComponent<AudioSource>();

        _lastBassTime = Time.time;
        _lastMidTime = Time.time;
    }

    // ---------------------------------------------
    // LOOP PRINCIPAL
    // ---------------------------------------------
    void Update()
    {
        if (_audioSource == null || !_audioSource.isPlaying) return;

        _audioSource.GetSpectrumData(_samples, 0, FFTWindow.BlackmanHarris);

        DetectBass();
        DetectMids();
    }

    // ---------------------------------------------
    // DETECCION DE BAJOS — KAMIKAZE
    // ---------------------------------------------
    void DetectBass()
    {
        float intensity = SumSamples(_bassStartSample, _bassEndSample);

        bool isPeak = intensity > _prevBassIntensity;
        bool overThresh = intensity > _bassThreshold;
        bool cooledDown = Time.time - _lastBassTime > _bassCooldown;

        if (isPeak && overThresh && cooledDown)
        {
            _beatCount = (_beatCount % _beatsPerMeasure) + 1;

            // Siempre spawnea Kamikaze en cada beat
            SpawnEnemy(_kamikazePrefab);

            // En el beat fuerte intenta spawnear Orbitador
            // pero respeta el intervalo minimo
            if (_beatCount == _beatsPerMeasure)
            {
                TrySpawnOrbitador();
            }

            _lastBassTime = Time.time;
        }

        _prevBassIntensity = intensity;
    }

    // ---------------------------------------------
    // DETECCION DE MEDIOS — ORBITADOR
    // ---------------------------------------------
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

    // ---------------------------------------------
    // SPAWN DEL ORBITADOR CON INTERVALO MINIMO
    // ---------------------------------------------

    /// <summary>
    /// Intenta spawnear el Orbitador solo si paso el intervalo minimo.
    /// Tanto DetectBass (beat fuerte) como DetectMids llaman a este metodo.
    /// </summary>
    void TrySpawnOrbitador()
    {
        bool intervalOk = Time.time - _lastOrbitadorTime > _orbitadorMinInterval;
        if (!intervalOk) return;

        SpawnEnemy(_circularEnemyPrefab);
        _lastOrbitadorTime = Time.time;
    }

    // ---------------------------------------------
    // UTILIDADES
    // ---------------------------------------------

    /// <summary>Suma los valores FFT en un rango de samples.</summary>
    float SumSamples(int from, int to)
    {
        float sum = 0f;
        int end = Mathf.Min(to, _samples.Length - 1);
        for (int i = from; i <= end; i++)
            sum += _samples[i];
        return sum;
    }

    /// <summary>Spawnea cualquier EnemyBase a traves del pool.</summary>
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