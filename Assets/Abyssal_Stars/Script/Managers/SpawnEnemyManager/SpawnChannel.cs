using UnityEngine;

[System.Serializable]
public class SpawnChannel
{
    [Tooltip("Nombre descriptivo (ej: Low, Mid, High, SubLow).")]
    public string name = "Channel";

    [Tooltip("Prefab del enemigo a spawnear.")]
    public EnemyBase prefab;

    [Tooltip("Zona de spawn — GameObject con componente SpawnZone.")]
    public SpawnZone zone;

    [Tooltip("Tiempo mínimo entre spawns (segundos).")]
    public float interval = 1f;

    [Tooltip("Máximo de enemigos activos de este canal. 0 = sin límite.")]
    public int maxActive = 0;

    [HideInInspector] public float lastSpawnTime = -99f;
    [HideInInspector] public int activeCount = 0;

    public bool IsReady()
    {
        if (Time.time - lastSpawnTime <= interval) return false;
        if (maxActive > 0 && activeCount >= maxActive) return false;
        return true;
    }

    public void MarkSpawned()
    {
        lastSpawnTime = Time.time;
        activeCount++;
    }

    public void MarkDefeated()
    {
        activeCount = Mathf.Max(0, activeCount - 1);
    }

    public Vector3 GetRandomPosition() =>
        zone != null ? zone.GetRandomPosition() : Vector3.zero;
}