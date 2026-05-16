using UnityEngine;
using System;
using System.Collections;

public class BossPhaseController : MonoBehaviour
{
    public static BossPhaseController Instance { get; private set; }

    [Header("Timing")]
    [SerializeField] private float _timeToBoss = 60f;
    [SerializeField] private float _warningDuration = 3f;
    public float WarningDuration => _warningDuration;

    [Header("Boss")]
    [SerializeField] private GameObject _bossPrefab;
    [SerializeField] private SpawnZone _bossSpawnZone;

    public static event Action OnBossWarning;

    public static event Action OnBossFightStarted;

    private float _timer = 0f;
    private bool _warningFired = false;
    private bool _bossFired = false;
    private bool _running = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    private void Start()
    {

        Begin();
    }

    private void Update()
    {
        if (!_running || _bossFired) return;

        _timer += Time.deltaTime;

        if (!_warningFired && _timer >= _timeToBoss - _warningDuration)
        {
            _warningFired = true;
            TriggerWarning();
        }

        if (_timer >= _timeToBoss)
        {
            _bossFired = true;
            StartCoroutine(SpawnBossRoutine());
        }
    }

    public void Begin() => _running = true;

    public void Pause() => _running = false;

    public void Resume() => _running = true;

    private void TriggerWarning()
    {
        OnBossWarning?.Invoke();

        if (BossWarningUI.Instance != null)
            BossWarningUI.Instance.ShowWarning(_warningDuration);
    }

    private IEnumerator SpawnBossRoutine()
    {
        yield return new WaitForSeconds(_warningDuration);

        if (_bossPrefab != null && _bossSpawnZone != null)
            Instantiate(_bossPrefab, _bossSpawnZone.GetRandomPosition(), Quaternion.identity);

        OnBossFightStarted?.Invoke();
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (_bossSpawnZone == null) return;

        Gizmos.color = new Color(1f, 0.15f, 0.15f, 0.4f);
        Vector3 size = new Vector3(_bossSpawnZone.rangeX * 2f, _bossSpawnZone.rangeY * 2f, 0f);
        Gizmos.DrawWireCube(_bossSpawnZone.transform.position, size);

        Color fill = new Color(1f, 0.15f, 0.15f, 0.08f);
        Gizmos.color = fill;
        Gizmos.DrawCube(_bossSpawnZone.transform.position, size);

        UnityEditor.Handles.color = Color.red;
        UnityEditor.Handles.Label(
            _bossSpawnZone.transform.position + Vector3.up * (_bossSpawnZone.rangeY + 0.25f),
            "Boss Zone"
        );
    }
#endif
}