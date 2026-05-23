using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "New Ring", menuName = "Boss Patterns/Attack/Ring")]
public class PatternRingSO : AttackPatternSO
{
    [Header("Configuración")]
    [Tooltip("Balas por anillo.")]
    [Min(3)] public int   bulletCount = 12;

    [Tooltip("Delay entre anillos.")]
    public float fireRate = 1f;

    [Tooltip("Velocidad de las balas.")]
    public float bulletSpeed = 4f;

    [Tooltip("Si está activo, cada anillo rota un poco respecto al anterior.")]
    public bool rotateRings = false;

    [Tooltip("Grados de rotación por anillo (solo si rotateRings está activo).")]
    public float rotationPerRing = 15f;

    public override IEnumerator ExecutePattern(BossTurret turret)
    {
        float currentOffset = 0f;

        while (true)
        {
            float angleStep = 360f / bulletCount;

            for (int i = 0; i < bulletCount; i++)
            {
                float angle = currentOffset + angleStep * i;
                turret.FireSingleBullet(ApplyMirror(angle), bulletSpeed);
            }

            if (rotateRings) currentOffset += rotationPerRing;

            yield return new WaitForSeconds(fireRate);
        }
    }
}
