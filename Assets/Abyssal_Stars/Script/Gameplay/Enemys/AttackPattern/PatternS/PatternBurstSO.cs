using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "New Burst", menuName = "Boss Patterns/Attack/Burst")]
public class PatternBurstSO : AttackPatternSO
{
    [Header("Configuración")]
    [Tooltip("Balas por ráfaga.")]
    [Min(1)] [SerializeField] private int   bulletsPerBurst = 4;

    [Tooltip("Delay entre balas dentro de la ráfaga.")]
    [SerializeField] private float inBurstDelay = 0.08f;

    [Tooltip("Pausa entre ráfagas.")]
    [SerializeField] private float pauseBetweenBursts = 0.8f;

    [Tooltip("Velocidad de las balas.")]
    [SerializeField] private float bulletSpeed = 6f;

    [Tooltip("Ángulo central del disparo. 270 = hacia abajo.")]
    [SerializeField] private float fireAngle = 270f;

    [Tooltip("Dispersión aleatoria por bala (grados). 0 = disparo perfectamente recto.")]
    [SerializeField] private float randomSpread = 5f;

    public override IEnumerator ExecutePattern(BossTurret turret)
    {
        while (true)
        {
            for (int i = 0; i < bulletsPerBurst; i++)
            {
                float angle = ApplyMirror(fireAngle + Random.Range(-randomSpread, randomSpread));
                turret.FireSingleBullet(angle, bulletSpeed);

                if (i < bulletsPerBurst - 1)
                    yield return new WaitForSeconds(inBurstDelay);
            }

            yield return new WaitForSeconds(pauseBetweenBursts);
        }
    }
}
