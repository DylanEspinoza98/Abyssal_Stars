using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "New Continuous Drop", menuName = "Boss Patterns/Attack/Continuous Drop")]
public class PatternContinuousDropSO : AttackPatternSO
{
    [Header("Disparo Continuo")]
    [SerializeField] private float fireRate = 0.4f;
    [SerializeField] private float bulletSpeed = 1.0f;

    [Tooltip("Ángulo de disparo. 270 = Abajo (como el original). 90 = Arriba (cola literal tras de él).")]
    [SerializeField] private float dropAngle = 270f;

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