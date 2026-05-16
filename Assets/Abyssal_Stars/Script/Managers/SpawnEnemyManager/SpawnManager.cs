using UnityEngine;
using System;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance { get; private set; }

    [Header("Canal 0: Low (Bajos / Bombo)")]
    [SerializeField] private SpawnChannel _channelLow = new SpawnChannel { name = "Low" };

    [Header("Canal 1: Mid (Medios / Sintetizador)")]
    [SerializeField] private SpawnChannel _channelMid = new SpawnChannel { name = "Mid" };

    [Header("Canal 2: High (Altos / Platillos)")]
    [SerializeField] private SpawnChannel _channelHigh = new SpawnChannel { name = "High" };

    [Header("Canal 3: SubLow (Sub-Bajos profundos)")]
    [SerializeField] private SpawnChannel _channelSubLow = new SpawnChannel { name = "SubLow" };

    private SpawnChannel[] _channels;

    public static event Action OnBossFightStarted;

    private bool _spawningPaused = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        _channels = new SpawnChannel[] { _channelLow, _channelMid, _channelHigh, _channelSubLow };
    }

    private void Start()
    {
        SubscribeToBeatDetector(subscribe: true);
        BossPhaseController.OnBossWarning += PauseSpawning;
        BossPhaseController.OnBossFightStarted += HandleBossFightStarted;
    }

    private void OnDestroy()
    {
        SubscribeToBeatDetector(subscribe: false);
        BossPhaseController.OnBossWarning -= PauseSpawning;
        BossPhaseController.OnBossFightStarted -= HandleBossFightStarted;
    }

    private void HandleBossFightStarted()
    {
        PauseSpawning();
        OnBossFightStarted?.Invoke();
    }

    public void PauseSpawning() => _spawningPaused = true;
    public void ResumeSpawning() => _spawningPaused = false;

    private void SubscribeToBeatDetector(bool subscribe)
    {
        AudioBeatDetector detector = AudioBeatDetector.Instance;
        if (detector == null) return;

        if (subscribe)
        {
            detector.OnLowBeat += HandleLow;
            detector.OnMidBeat += HandleMid;
            detector.OnHighBeat += HandleHigh;
            detector.OnSubLowBeat += HandleSubLow;
        }
        else
        {
            detector.OnLowBeat -= HandleLow;
            detector.OnMidBeat -= HandleMid;
            detector.OnHighBeat -= HandleHigh;
            detector.OnSubLowBeat -= HandleSubLow;
        }
    }

    private void HandleLow() => TrySpawn(0);
    private void HandleMid() => TrySpawn(1);
    private void HandleHigh() => TrySpawn(2);
    private void HandleSubLow() => TrySpawn(3);

    private void TrySpawn(int channelIndex)
    {
        if (_spawningPaused) return;
        if (_channels == null || channelIndex >= _channels.Length) return;

        SpawnChannel channel = _channels[channelIndex];

        if (channel == null || !channel.IsReady()) return;

        SpawnEnemy(channel);
    }

    private void SpawnEnemy(SpawnChannel channel)
    {
        if (channel.prefab == null || channel.zone == null) return;

        Vector3 finalPos = channel.GetRandomPosition();
        EnemyPool.Instance.GetEnemy(channel.prefab, finalPos, channel.prefab.transform.rotation);
        channel.MarkSpawned();
    }

    public static void NotifyDefeated(GameObject prefabKey)
    {
        if (Instance == null || Instance._channels == null || prefabKey == null) return;

        foreach (SpawnChannel channel in Instance._channels)
        {
            if (channel != null && channel.prefab != null &&
                channel.prefab.gameObject == prefabKey)
            {
                channel.MarkDefeated();
                return;
            }
        }
    }
}