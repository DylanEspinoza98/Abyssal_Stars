using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "New Butterfly", menuName = "Boss Patterns/Attack/Butterfly")]
public class PatternButterflySO : AttackPatternSO
{
    [Header("Espirales")]
    [Tooltip("Brazos por espiral. 1 = mariposa simple, 2 = doble, 3 = flor.")]
    [Min(1)]
    public int armsPerSpiral = 1;

    [Tooltip("Velocidad de rotación (grados/salva). Cada espiral rota en sentido opuesto.")]
    public float spinSpeed = 12f;

    [Tooltip("Velocidad de las balas.")]
    public float bulletSpeed = 5f;

    [Tooltip("Delay entre salvas.")]
    public float fireRate = 0.07f;

    [Header("Separación entre espirales")]
    [Tooltip("Ángulo de offset entre las dos alas (180 = perfectamente simétricas).")]
    [Range(90f, 180f)]
    public float wingOffset = 180f;

    [Header("Pulso")]
    [Tooltip("Si está activo, la velocidad de spin oscila creando un efecto de aleteo.")]
    public bool enableFlap = false;

    [Tooltip("Amplitud del aleteo (varía el spinSpeed en ±flapAmount).")]
    public float flapAmount = 8f;

    [Tooltip("Velocidad del aleteo.")]
    public float flapSpeed = 2f;

    public override IEnumerator ExecutePattern(BossTurret turret)
    {
        float angleA    = 0f; 
        float angleB    = wingOffset;
        float timeAccum = 0f;
        float angleStep = 360f / armsPerSpiral;

        while (true)
        {
            timeAccum += fireRate;

            float currentSpin = spinSpeed;
            if (enableFlap)
                currentSpin += Mathf.Sin(timeAccum * flapSpeed) * flapAmount;

            for (int i = 0; i < armsPerSpiral; i++)
                turret.FireSingleBullet(
                    ApplyMirror(angleA + angleStep * i),
                    bulletSpeed
                );

            for (int i = 0; i < armsPerSpiral; i++)
                turret.FireSingleBullet(
                    ApplyMirror(angleB - angleStep * i),
                    bulletSpeed
                );

            angleA += currentSpin;
            angleB -= currentSpin;

            yield return new WaitForSeconds(fireRate);
        }
    }
}
