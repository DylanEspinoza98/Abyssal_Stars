using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Depth Burst", menuName = "Boss Patterns/Attack/Depth Burst")]
public class PatternDepthBurstSO : AttackPatternSO
{
    [Header("Proyectil de Fondo")]
    [SerializeField] private GameObject depthProjectilePrefab;

    [Header("Grid Hexagonal")]
    [SerializeField] private int hexRings = 2;
    [SerializeField] private float hexSpacing = 0.5f;

    [Header("Configuración de Disparo")]
    [Tooltip("Tiempo en segundos entre la creación de un hexágono y otro.")]
    [SerializeField] private float delayBetweenBursts = 1.5f;
    [Tooltip("Qué tan rápido gira el hexágono sobre sí mismo (grados por segundo).")]
    [SerializeField] private float hexRotationSpeed = 90f;

    public override IEnumerator ExecutePattern(BossTurret turret)
    {
        Transform playerTransform = GameObject.FindWithTag("Player")?.transform;
        List<Vector2> hexOffsets = BuildHexGrid(hexRings, hexSpacing);

        while (true)
        {
            while (!DepthPhaseSignal.IsActive)
                yield return null;

            while (DepthPhaseSignal.IsActive)
            {
                if (playerTransform != null)
                {
                    Vector2 snapshotPos = playerTransform.position;
                    FireHexBurst(turret, snapshotPos, hexOffsets);
                }

                yield return new WaitForSeconds(delayBetweenBursts);
            }
        }
    }

    private void FireHexBurst(BossTurret turret, Vector2 snapshotPlayerPos, List<Vector2> offsets)
    {
        Vector2 turretPos = (Vector2)turret.transform.position;
        Vector2 direction = snapshotPlayerPos - turretPos;
        if (direction.sqrMagnitude < 0.001f) direction = Vector2.down;
        direction.Normalize();

        // Ángulo inicial para apuntar la malla hacia el jugador
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        Quaternion gridRotation = Quaternion.Euler(0f, 0f, angle);

        Vector2 spawnPos = turret.transform.position;

        // La velocidad de avance es dinámica (calculada por la distancia y el tiempo de expansión)
        float distance = Vector2.Distance(spawnPos, snapshotPlayerPos);

        foreach (Vector2 offset in offsets)
        {
            // Calculamos el offset local (con el grid rotado hacia el objetivo)
            Vector2 localOffset = (Vector2)(gridRotation * offset);

            GameObject bulletObj = Instantiate(depthProjectilePrefab, spawnPos, Quaternion.identity);
            DepthProjectile dp = bulletObj.GetComponent<DepthProjectile>();

            if (dp != null)
            {
                float dynamicSpeed = distance / dp.TimeToForeground;

                // INYECCIÓN ORBITAL: Le pasamos toda la responsabilidad a la bala
                dp.FireHexGrid(spawnPos, direction, dynamicSpeed, localOffset, hexRotationSpeed);
            }
        }
    }

    private List<Vector2> BuildHexGrid(int rings, float spacing)
    {
        List<Vector2> offsets = new List<Vector2>();

        for (int q = -rings; q <= rings; q++)
        {
            int rMin = Mathf.Max(-rings, -q - rings);
            int rMax = Mathf.Min(rings, -q + rings);

            for (int r = rMin; r <= rMax; r++)
            {
                float x = spacing * 1.5f * q;
                float y = spacing * (Mathf.Sqrt(3f) / 2f * q + Mathf.Sqrt(3f) * r);
                offsets.Add(new Vector2(x, y));
            }
        }

        return offsets;
    }
}