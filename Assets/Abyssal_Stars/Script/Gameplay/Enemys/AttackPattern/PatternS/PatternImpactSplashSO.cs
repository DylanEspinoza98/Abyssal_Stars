using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "New Impact Splash", menuName = "Boss Patterns/Attack/Impact Splash")]
public class PatternImpactSplashSO : AttackPatternSO
{
    [Header("Disparo en el Aire (Goteo)")]
    public float fireRate = 0.5f;
    public float bulletSpeed = 2.0f;

    [Header("El Estallido (Al tocar el suelo)")]
    [Tooltip("La altura a la que se considera que el jefe chocó (ej. -5.5)")]
    public float impactYThreshold = -5.5f;
    public int impactBulletsCount = 12;
    public float impactBulletSpeed = 5f;

    public override IEnumerator ExecutePattern(BossTurret turret)
    {
        if (turret == null) yield break;

        bool hasImpacted = false;
        float dropTimer = 0f;

        while (true)
        {
            float currentY = turret.transform.position.y;

            if (currentY <= impactYThreshold && !hasImpacted)
            {
                FireRadialBurst(turret);
                hasImpacted = true;
            }
            else if (currentY > impactYThreshold)
            {
                hasImpacted = false;

                dropTimer += Time.deltaTime;
                if (dropTimer >= fireRate)
                {
                    turret.FireSingleBullet(270f, bulletSpeed);
                    dropTimer = 0f;
                }
            }

            yield return null;
        }
    }

    private void FireRadialBurst(BossTurret turret)
    {
        float angleStep = 360f / impactBulletsCount;

        for (int i = 0; i < impactBulletsCount; i++)
        {
            float randomAngleOffset = Random.Range(-15f, 15f);
            float finalAngle = (i * angleStep) + randomAngleOffset;

            float randomSpeedMultiplier = Random.Range(0.5f, 1.5f);
            float finalSpeed = impactBulletSpeed * randomSpeedMultiplier;

            turret.FireSingleBullet(finalAngle, finalSpeed);
        }
    }
}