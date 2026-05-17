using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "New Cage", menuName = "Boss Patterns/Attack/Cage")]
public class PatternCageSO : AttackPatternSO
{
    [Header("Jaula")]
    [Tooltip("Balas por pared de la jaula.")]
    [Min(2)] public int bulletsPerWall = 5;

    [Tooltip("Separación entre balas de una misma pared.")]
    public float bulletSpacing = 0.5f;

    [Tooltip("Velocidad de las balas de la jaula.")]
    public float bulletSpeed = 4f;

    [Tooltip("Pausa entre jaulas.")]
    public float cageCooldown = 3f;

    [Tooltip("Tag del jugador para apuntar la jaula.")]
    public string playerTag = "Player";

    public override IEnumerator ExecutePattern(BossTurret turret)
    {
        while (true)
        {
            yield return new WaitForSeconds(cageCooldown);

            // Buscar posición del jugador
            GameObject player = GameObject.FindWithTag(playerTag);
            Vector2 targetPos = player != null
                ? (Vector2)player.transform.position
                : (Vector2)turret.transform.position + Vector2.down * 3f;

            SpawnCage(turret, targetPos);
        }
    }

    private void SpawnCage(BossTurret turret, Vector2 center)
    {
        // 4 paredes: arriba, abajo, izquierda, derecha
        // Cada pared dispara hacia el centro de la jaula

        float halfWidth  = (bulletsPerWall - 1) * bulletSpacing * 0.5f;

        // Pared superior → dispara hacia abajo (270°)
        for (int i = 0; i < bulletsPerWall; i++)
        {
            float offsetX = -halfWidth + i * bulletSpacing;
            FireCageBullet(turret, center + new Vector2(offsetX, 4f), 270f);
        }

        // Pared inferior → dispara hacia arriba (90°)
        for (int i = 0; i < bulletsPerWall; i++)
        {
            float offsetX = -halfWidth + i * bulletSpacing;
            FireCageBullet(turret, center + new Vector2(offsetX, -4f), 90f);
        }

        // Pared izquierda → dispara hacia la derecha (0°)
        for (int i = 0; i < bulletsPerWall; i++)
        {
            float offsetY = -halfWidth + i * bulletSpacing;
            FireCageBullet(turret, center + new Vector2(-4f, offsetY), 0f);
        }

        // Pared derecha → dispara hacia la izquierda (180°)
        for (int i = 0; i < bulletsPerWall; i++)
        {
            float offsetY = -halfWidth + i * bulletSpacing;
            FireCageBullet(turret, center + new Vector2(4f, offsetY), 180f);
        }
    }

    /// <summary>
    /// Dispara una bala desde una posición en el mundo (no desde la torreta).
    /// Usamos BulletPool directamente ya que necesitamos posición arbitraria.
    /// </summary>
    private void FireCageBullet(BossTurret turret, Vector2 worldPosition, float angle)
    {
        if (BulletPool.Instance == null || turret.bulletPrefab == null) return;

        float   rad = angle * Mathf.Deg2Rad;
        Vector2 dir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;

        EnemyBullet bullet = BulletPool.Instance.GetBullet(
            turret.bulletPrefab,
            worldPosition,
            Quaternion.identity,
            dir * bulletSpeed
        );

        if (bullet == null) return;

        bullet.SetRotationByVelocity();
        if (turret.bulletSprite != null)
            bullet.SetAppearance(turret.bulletSprite, turret.bulletColor);
    }
}
