using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class BackgroundSpawner : MonoBehaviour
{

    public static BackgroundSpawner Instance { get; private set; }

    [Header("Capas de Parallax")]
    [Tooltip("Asigna aquí los GameObjects hijos que tienen BackgroundLayer.")]
    [SerializeField] private BackgroundLayer[] _layers;

    [Header("Pre-Warm")]
    [SerializeField] private bool _preWarmOnStart = true;
    [SerializeField] private int _preWarmPerLayer = 4;
    [SerializeField] private float _spawnTopY = 6f;
    [SerializeField] private float _killY = -50f;
    public float KillY => _killY;

    public event Action OnPreWarmComplete;


    public event Action<string, Vector3> OnLayerSpawned;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (_layers == null || _layers.Length == 0)
            _layers = GetComponentsInChildren<BackgroundLayer>();
    }

    private void Start()
    {
        if (_layers != null)
            foreach (var layer in _layers)
                layer?.SetKillY(_killY);

        if (_preWarmOnStart)
            StartCoroutine(PreWarmRoutine());
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private IEnumerator PreWarmRoutine()
    {
        if (_layers == null || _layers.Length == 0)
        {
            OnPreWarmComplete?.Invoke();
            yield break;
        }

        float screenTop = _spawnTopY;
        float screenBottom = _killY + 2f;

        for (int li = 0; li < _layers.Length; li++)
        {
            BackgroundLayer layer = _layers[li];
            if (layer == null) continue;

            for (int i = 0; i < _preWarmPerLayer; i++)
            {
                float t = (float)i / Mathf.Max(_preWarmPerLayer - 1, 1);
                float baseY = Mathf.Lerp(screenTop, screenBottom, t);
                float noisyY = baseY + UnityEngine.Random.Range(-1.2f, 1.2f);

                bool forceCommon = (i < 2);
                layer.SpawnAt(noisyY, forceCommon);
            }

            yield return null;
        }

        OnPreWarmComplete?.Invoke();
        Debug.Log("[BackgroundSpawner] Pre-warm completado.");
    }
    public void ResetAllLayers()
    {
        if (_layers == null) return;
        foreach (var layer in _layers)
            layer?.ReturnAll();
    }
    public void SetLayerActive(string layerName, bool active)
    {
        if (_layers == null) return;
        foreach (var layer in _layers)
        {
            if (layer != null && layer.layerName == layerName)
                layer.gameObject.SetActive(active);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // Línea de spawn top
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(new Vector3(-10f, _spawnTopY, 0f), new Vector3(10f, _spawnTopY, 0f));
        UnityEditor.Handles.Label(new Vector3(-9f, _spawnTopY + 0.2f, 0f), "Spawn Top");

        // Línea de kill
        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(-10f, _killY, 0f), new Vector3(10f, _killY, 0f));
        UnityEditor.Handles.Label(new Vector3(-9f, _killY + 0.2f, 0f), "Kill Y");
    }
#endif
}