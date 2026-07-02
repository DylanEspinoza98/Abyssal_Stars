using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "New Spiral", menuName = "Boss Patterns/Attack/Spiral")]
public class PatternSpiralSO : AttackPatternSO
{
    [Header("Configuración")]
    [Tooltip("Delay entre cada salva de brazos (segundos).")]
    public float fireRate = 0.08f;

    [Tooltip("Velocidad de las balas.")]
    public float bulletSpeed = 5f;

    [Tooltip("Cantidad de brazos de la espiral.")]
    [Min(1)]
    public int arms = 3;

    [Tooltip("Velocidad de rotación (grados/salva). Positivo = antihorario, negativo = horario.")]
    public float spinSpeed = 15f;

    [Tooltip("Si está activo, invierte la dirección de giro cada vuelta completa (360°).")]
    public bool alternateDirection = false;

    public override IEnumerator ExecutePattern(BossTurret turret)
    {
        float currentAngle = 0f;
        float angleStep = 360f / arms;
        float currentSpin = spinSpeed;
        float accumulated = 0f; 

        while (true)
        {
            for (int i = 0; i < arms; i++)
                turret.FireSingleBullet(ApplyMirror(currentAngle + angleStep * i), bulletSpeed);

            currentAngle += currentSpin;
            accumulated += Mathf.Abs(currentSpin);

            if (alternateDirection && accumulated >= 360f)
            {
                accumulated = 0f;
                currentSpin = -currentSpin;
            }

            yield return new WaitForSeconds(fireRate);
        }
    }
}