using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "New Continuous Drop", menuName = "Boss Patterns/Attack/Continuous Drop")]
public class PatternContinuousDropSO : AttackPatternSO
{
    [Header("Disparo Continuo")]
    public float fireRate = 0.4f;
    public float bulletSpeed = 1.0f;

    [Tooltip("Ángulo de disparo. 270 = Abajo (como el original). 90 = Arriba (cola literal tras de él).")]
    public float dropAngle = 270f;

    public override IEnumerator ExecutePattern(BossTurret turret)
    {
        if (turret == null) yield break;

        while (true)
        {
            turret.FireSingleBullet(ApplyMirror(dropAngle), bulletSpeed);
            yield return new WaitForSeconds(fireRate);
        }
    }
}