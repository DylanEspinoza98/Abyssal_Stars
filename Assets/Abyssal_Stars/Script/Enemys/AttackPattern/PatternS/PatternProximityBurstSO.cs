using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "New Proximity Burst", menuName = "Boss Patterns/Attack/Proximity Burst")]
public class PatternProximityBurstSO : AttackPatternSO
{
    [Header("Disparo por Proximidad")]
    public float triggerRange = 10f;
    public int shotsPerBurst = 3;
    public float timeBetweenShots = 0.15f;
    public float bulletSpeed = 8f;

    [Header("Tiempos y Recarga")]
    [Tooltip("Tiempo de espera (en segundos) tras detectar al jugador antes de disparar. Útil para sincronizar la animación.")]
    public float chargeTime = 0.5f;

    [Tooltip("Tiempo de espera antes de poder detectar y disparar OTRA ráfaga.")]
    public float cooldownBetweenBursts = 2.0f;

    [Header("Comportamiento")]
    [Tooltip("True: Cada bala apunta a la nueva posición del jugador. False: Toda la ráfaga va hacia donde estaba el jugador al iniciar.")]
    public bool trackPlayerDuringBurst = false;

    public override IEnumerator ExecutePattern(BossTurret turret)
    {
        if (turret == null) yield break;

        float sqrTriggerRange = triggerRange * triggerRange;
        Transform turretTransform = turret.transform;

        while (true)
        {
            if (PlayerHealth.Instance != null)
            {
                float sqrDistance = (PlayerHealth.Instance.transform.position - turretTransform.position).sqrMagnitude;

                if (sqrDistance <= sqrTriggerRange)
                {
                    if (chargeTime > 0f)
                    {
                        yield return new WaitForSeconds(chargeTime);
                    }

                    yield return FireBurst(turret, turretTransform);

                    if (cooldownBetweenBursts > 0f)
                    {
                        yield return new WaitForSeconds(cooldownBetweenBursts);
                    }
                }
            }
            yield return null;
        }
    }

    private IEnumerator FireBurst(BossTurret turret, Transform turretTransform)
    {
        WaitForSeconds wait = new WaitForSeconds(timeBetweenShots);
        Vector2 lockedDirection = Vector2.zero;

        if (!trackPlayerDuringBurst && PlayerHealth.Instance != null)
        {
            lockedDirection = (PlayerHealth.Instance.transform.position - turretTransform.position).normalized;
        }

        for (int i = 0; i < shotsPerBurst; i++)
        {
            if (PlayerHealth.Instance != null)
            {
                Vector2 currentDirection = lockedDirection;

                if (trackPlayerDuringBurst)
                {
                    currentDirection = (PlayerHealth.Instance.transform.position - turretTransform.position).normalized;
                }

                if (currentDirection != Vector2.zero)
                {
                    float angle = Mathf.Atan2(currentDirection.y, currentDirection.x) * Mathf.Rad2Deg;
                    turret.FireSingleBullet(ApplyMirror(angle), bulletSpeed);
                }
            }

            if (i < shotsPerBurst - 1)
            {
                yield return wait;
            }
        }
    }
}