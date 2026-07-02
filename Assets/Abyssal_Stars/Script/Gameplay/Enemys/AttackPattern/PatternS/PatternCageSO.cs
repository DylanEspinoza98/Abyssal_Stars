using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "New Circular Cage", menuName = "Boss Patterns/Attack/Circular Cage")]
public class PatternCageSO : AttackPatternSO
{
    [Header("Jaula Circular (Estilo Asgore)")]
    [Tooltip("Cantidad total de balas si el círculo estuviera completo.")]
    [Min(10)] [SerializeField] private int bulletsPerRing = 24;

    [Tooltip("Radio desde el que spawnan las balas. ¡Cuidado con el borde de cámara!")]
    [SerializeField] private float spawnRadius = 7f;

    [Tooltip("Tamaño del hueco de escape en grados.")]
    [Range(30f, 120f)] [SerializeField] private float gapSizeDegrees = 60f;

    [Tooltip("Velocidad a la que se cierra la jaula.")]
    [SerializeField] private float bulletSpeed = 5f;

    [Header("Kill Point (centro de la jaula)")]
    [Tooltip("Radio alrededor del centro en el que las balas desaparecen al converger. " +
             "Ajustá según el tamaño visual de tu jugador.")]
    [Range(0.1f, 2f)] [SerializeField] private float killRadius = 0.4f;

    [Header("Timing de Disparo")]
    [Tooltip("Cuántos anillos dispara por cada ataque.")]
    [SerializeField] private int ringsPerAttack = 3;

    [Tooltip("Pausa rápida entre cada anillo del mismo combo.")]
    [SerializeField] private float timeBetweenRings = 0.5f;

    [Tooltip("Pausa larga antes de empezar el siguiente combo.")]
    [SerializeField] private float cageCooldown = 2f;

    [Tooltip("Segundos que las balas 'esperan' antes de moverse (aviso visual).")]
    [Range(0f, 1f)] [SerializeField] private float warmupDuration = 0.25f;

    [Header("Dificultad del Hueco")]
    [Tooltip("Offset máximo en grados del hueco respecto al jugador. " +
             "0 = siempre apunta al jugador; 90 = hasta 90° de variación.")]
    [Range(0f, 90f)] [SerializeField] private float gapAngleVariance = 30f;

    [Tooltip("Rotación adicional entre anillos del mismo combo (grados).")]
    [Range(0f, 45f)] [SerializeField] private float ringRotationStep = 15f;

    [Header("Centro de la Jaula")]
    [Tooltip("Desplazamiento vertical desde el centro de la cámara. " +
             "Negativo = más abajo. Ej: -1.5 pone la jaula un poco bajo el centro.")]
    [SerializeField] private float verticalOffset = -1.5f;

    private Vector2 GetCageCenter()
    {
        Camera cam = Camera.main;
        if (cam == null) return Vector2.zero;

        Vector3 camCenter = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, cam.nearClipPlane));
        return new Vector2(camCenter.x, camCenter.y + verticalOffset);
    }
    public override IEnumerator ExecutePattern(BossTurret turret)
    {
        while (true)
        {
            Vector2 center = GetCageCenter();

            float baseGapAngle = Random.Range(0f, 360f);

            for (int r = 0; r < ringsPerAttack; r++)
            {
                float variance = Random.Range(-gapAngleVariance, gapAngleVariance);
                float gapAngle = baseGapAngle + variance + (r * ringRotationStep);

                yield return SpawnCageRing(turret, center, gapAngle);

                if (r < ringsPerAttack - 1)
                    yield return new WaitForSeconds(timeBetweenRings);
            }

            yield return new WaitForSeconds(cageCooldown);
        }
    }

    private IEnumerator SpawnCageRing(BossTurret turret, Vector2 center, float gapCenterAngle)
    {
        float angleStep = 360f / bulletsPerRing;
        float halfGap = gapSizeDegrees / 2f;

        for (int i = 0; i < bulletsPerRing; i++)
        {
            float currentAngle = i * angleStep;
            if (Mathf.Abs(Mathf.DeltaAngle(currentAngle, gapCenterAngle)) < halfGap)
                continue;

            float rad = currentAngle * Mathf.Deg2Rad;
            Vector2 spawnPos = center + new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * spawnRadius;
            SpawnWarmupBullet(turret, spawnPos, center);
        }

        if (warmupDuration > 0f)
            yield return new WaitForSeconds(warmupDuration);

        for (int i = 0; i < bulletsPerRing; i++)
        {
            float currentAngle = i * angleStep;
            if (Mathf.Abs(Mathf.DeltaAngle(currentAngle, gapCenterAngle)) < halfGap)
                continue;

            float rad = currentAngle * Mathf.Deg2Rad;
            Vector2 spawnPos = center + new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)) * spawnRadius;
            float fireAngle = currentAngle + 180f;

            FireCageBullet(turret, spawnPos, fireAngle, center);
        }
    }

    private void SpawnWarmupBullet(BossTurret turret, Vector2 worldPosition, Vector2 center)
    {
        if (BulletPool.Instance == null || turret.BulletPrefab == null) return;

        EnemyBullet bullet = BulletPool.Instance.GetBullet(
            turret.BulletPrefab, worldPosition, Quaternion.identity, Vector2.zero
        );

        if (bullet == null) return;

        bullet.SetKillPoint(center, killRadius);

        if (turret.BulletSprite != null)
            bullet.SetAppearance(turret.BulletSprite, turret.BulletColor);
    }

    private void FireCageBullet(BossTurret turret, Vector2 worldPosition, float angle, Vector2 center)
    {
        if (BulletPool.Instance == null || turret.BulletPrefab == null) return;

        float rad = angle * Mathf.Deg2Rad;
        Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;

        EnemyBullet bullet = BulletPool.Instance.GetBullet(
            turret.BulletPrefab, worldPosition, Quaternion.identity, dir * bulletSpeed
        );

        if (bullet == null) return;

        bullet.SetKillPoint(center, killRadius);

        bullet.SetRotationByVelocity();
        if (turret.BulletSprite != null)
            bullet.SetAppearance(turret.BulletSprite, turret.BulletColor);
    }
}