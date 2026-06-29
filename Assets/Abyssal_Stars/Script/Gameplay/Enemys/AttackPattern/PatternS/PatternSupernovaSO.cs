using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;

[CreateAssetMenu(fileName = "New Supernova", menuName = "Boss Patterns/Attack/Multi Supernova")]
public class PatternSupernovaSO : AttackPatternSO
{
    [Header("Olas de Viaje")]
    public GameObject waveCollapsePrefab;

    [Tooltip("Total de espacios (balas + huecos). Lo ideal es que sea múltiplo de (fakeCount + lethalCount).")]
    [Min(2)]
    public int bulletsPerWave = 24;
    public float waveAmplitude = 1.5f;
    public float waveFrequency = 4f;
    public float bulletSpeed = 6f;
    public float spawnInterval = 0.12f;

    [Header("Latigazo")]
    public float rodLength = 1.5f;
    public float rotationSpeed = 90f;
    public float totalRotationDegrees = 360f;
    public float rodInitialAngle = 90f;
    public bool randomizeInitialAngle = false;

    [Header("Colapso")]
    [Min(1f)]
    public float collapseSpeedMult = 2.5f;

    [Header("Patrón Huecos / Letal")]
    [Tooltip("Cantidad de espacios vacíos (huecos) que dejará la ola.")]
    [Min(0)] public int fakeCount = 3;
    [Tooltip("Cantidad de balas reales seguidas.")]
    [Min(1)] public int lethalCount = 3;

    [Header("Anclas (un latigazo por cada punto)")]
    public Vector2[] safeAnchors = new Vector2[]
    {
        new Vector2( 2.5f,  1.5f),
        new Vector2(-2.5f,  1.5f),
        new Vector2(-2.5f, -1.5f),
        new Vector2( 2.5f, -1.5f),
    };

    public enum SpawnEdge { Horizontal, Vertical, RandomSide, Left, Right, Top, Bottom }

    [Header("Origen de las Olas")]
    public SpawnEdge spawnEdge = SpawnEdge.Horizontal;
    public float offscreenDistance = 10f;

    [Header("Supernova")]
    public GameObject depthProjectilePrefab;
    public int shockwaveBulletCount = 30;
    public float shockwaveSpeed = 8f;

    [Header("Seguridad")]
    public float maxWaitTime = 15f;

    public override IEnumerator ExecutePattern(BossTurret turret)
    {
        int waveCount = safeAnchors.Length;

        float[] rodAngles = new float[waveCount];
        for (int w = 0; w < waveCount; w++)
            rodAngles[w] = randomizeInitialAngle ? Random.Range(0f, 360f) : rodInitialAngle;

        var waveBullets = new List<WaveCollapseBullet>[waveCount];
        for (int w = 0; w < waveCount; w++)
            waveBullets[w] = new List<WaveCollapseBullet>(bulletsPerWave);

        int[] onRodCounts = new int[waveCount];
        int[] collapsedCounts = new int[waveCount];

        Vector2[] startPositions = new Vector2[waveCount];
        for (int w = 0; w < waveCount; w++)
            startPositions[w] = GetSpawnPosition(safeAnchors[w]);

        int cycleLen = fakeCount + lethalCount;

        // 1. CALCULAMOS CUÁNTAS BALAS REALES VAN A EXISTIR
        int actualBulletsToSpawn = 0;
        for (int i = 0; i < bulletsPerWave; i++)
        {
            bool isFake = (fakeCount > 0) && ((i % cycleLen) < fakeCount);
            if (!isFake) actualBulletsToSpawn++;
        }

        void HandleArrived(WaveCollapseBullet bullet, int wIdx)
        {
            waveBullets[wIdx].Add(bullet);
            onRodCounts[wIdx]++;
        }

        void HandleCollapsed(int idx)
        {
            collapsedCounts[idx]++;
            // 2. LA EXPLOSIÓN AHORA ESPERA A LAS BALAS REALES, NO AL TOTAL
            if (collapsedCounts[idx] == actualBulletsToSpawn)
                FireSupernova(safeAnchors[idx], turret);
        }

        for (int i = 0; i < bulletsPerWave; i++)
        {
            bool isFake = (fakeCount > 0) && ((i % cycleLen) < fakeCount);

            // 3. SI ES FAKE, DEJAMOS EL HUECO (SOLO ESPERAMOS EL TIEMPO Y SALTAMOS)
            if (isFake)
            {
                yield return new WaitForSeconds(spawnInterval);
                continue;
            }

            // Calculamos el radio de forma normal para que los huecos también existan en el latigazo
            float rodRadius = (1f - (float)i / Mathf.Max(bulletsPerWave - 1, 1)) * rodLength;

            for (int w = 0; w < waveCount; w++)
            {
                int wCapture = w;
                Vector2 dir = safeAnchors[w] - startPositions[w];

                GameObject obj = Instantiate(waveCollapsePrefab, startPositions[w], Quaternion.identity);
                WaveCollapseBullet wb = obj.GetComponent<WaveCollapseBullet>();
                if (wb == null) continue;

                wb.SetCollapseParameters(
                    waveAmplitude, waveFrequency,
                    dir, bulletSpeed,
                    safeAnchors[w], w,
                    rodAngles, rodRadius,
                    collapseSpeedMult,
                    b => HandleArrived(b, wCapture));

                wb.OnCollapseCompleteWithIndex += HandleCollapsed;
            }

            yield return new WaitForSeconds(spawnInterval);
        }

        float timer = 0f;
        while (timer < maxWaitTime)
        {
            bool allOnRod = true;
            for (int w = 0; w < waveCount; w++)
                if (onRodCounts[w] < actualBulletsToSpawn) { allOnRod = false; break; } // Usamos actualBulletsToSpawn
            if (allOnRod) break;
            timer += Time.deltaTime;
            yield return null;
        }

        float totalRotated = 0f;
        while (totalRotated < totalRotationDegrees)
        {
            float delta = rotationSpeed * Time.deltaTime;
            totalRotated += delta;
            for (int w = 0; w < waveCount; w++)
                rodAngles[w] += delta;
            yield return null;
        }

        for (int w = 0; w < waveCount; w++)
            foreach (WaveCollapseBullet wb in waveBullets[w])
                wb?.StartCollapsing();

        timer = 0f;
        while (timer < maxWaitTime)
        {
            bool allDone = true;
            for (int w = 0; w < waveCount; w++)
                if (collapsedCounts[w] < actualBulletsToSpawn) { allDone = false; break; } // Usamos actualBulletsToSpawn
            if (allDone) break;
            timer += Time.deltaTime;
            yield return null;
        }
    }

    private Vector2 GetSpawnPosition(Vector2 anchor)
    {
        float d = offscreenDistance;
        switch (spawnEdge)
        {
            case SpawnEdge.Left: return new Vector2(-d, anchor.y + Random.Range(-1f, 1f));
            case SpawnEdge.Right: return new Vector2(d, anchor.y + Random.Range(-1f, 1f));
            case SpawnEdge.Top: return new Vector2(anchor.x + Random.Range(-1f, 1f), d);
            case SpawnEdge.Bottom: return new Vector2(anchor.x + Random.Range(-1f, 1f), -d);
            case SpawnEdge.Vertical:
                {
                    float s = Random.value > 0.5f ? d : -d;
                    return new Vector2(anchor.x + Random.Range(-1f, 1f), s);
                }
            case SpawnEdge.RandomSide:
                {
                    int edge = Random.Range(0, 4);
                    if (edge == 0) return new Vector2(-d, anchor.y + Random.Range(-1f, 1f));
                    if (edge == 1) return new Vector2(d, anchor.y + Random.Range(-1f, 1f));
                    if (edge == 2) return new Vector2(anchor.x + Random.Range(-1f, 1f), d);
                    return new Vector2(anchor.x + Random.Range(-1f, 1f), -d);
                }
            default: // Horizontal
                {
                    float s = Random.value > 0.5f ? d : -d;
                    return new Vector2(s, anchor.y + Random.Range(-1f, 1f));
                }
        }
    }

    private void FireSupernova(Vector2 pos, BossTurret turret)
    {
        float step = 360f / shockwaveBulletCount;
        for (int i = 0; i < shockwaveBulletCount; i++)
        {
            Vector2 dir = Quaternion.Euler(0, 0, i * step) * Vector2.up;
            GameObject obj = Instantiate(depthProjectilePrefab, pos, Quaternion.identity);
            DepthProjectile dp = obj.GetComponent<DepthProjectile>();
            dp?.Fire(dir, shockwaveSpeed);
        }
    }

}