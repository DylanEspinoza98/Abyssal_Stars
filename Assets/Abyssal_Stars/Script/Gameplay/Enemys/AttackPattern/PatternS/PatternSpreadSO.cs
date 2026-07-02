using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "New Spread", menuName = "Boss Patterns/Attack/Spread")]
public class PatternSpreadSO : AttackPatternSO
{
    [Header("Configuración")]
    [Tooltip("Delay entre cada abanico (segundos).")]
    [SerializeField] private float fireRate = 0.6f;

    [Tooltip("Velocidad de las balas.")]
    [SerializeField] private float bulletSpeed = 5f;

    [Tooltip("Cantidad de balas por abanico.")]
    [Min(1)]
    [SerializeField] private int bulletCount = 5;

    [Tooltip("Ángulo total del abanico (grados).")]
    [Range(10f, 360f)]
    [SerializeField] private float spreadAngle = 60f;

    [Tooltip("Ángulo central del abanico. 270 = hacia abajo.")]
    [SerializeField] private float centerAngle = 270f;

    [Tooltip("Si está activo, alterna la dirección del barrido (izq→der / der→izq) en cada disparo.")]
    [SerializeField] private bool alternateDirection = false;

    public override IEnumerator ExecutePattern(BossTurret turret)
    {
        bool leftToRight = true;

        while (true)
        {
            float startAngle = centerAngle - spreadAngle * 0.5f;
            float step = bulletCount > 1 ? spreadAngle / (bulletCount - 1) : 0f;

            if (alternateDirection && !leftToRight)
            {
                for (int i = bulletCount - 1; i >= 0; i--)
                    turret.FireSingleBullet(ApplyMirror(startAngle + step * i), bulletSpeed);
            }
            else
            {
                for (int i = 0; i < bulletCount; i++)
                    turret.FireSingleBullet(ApplyMirror(startAngle + step * i), bulletSpeed);
            }

            if (alternateDirection) leftToRight = !leftToRight;

            yield return new WaitForSeconds(fireRate);
        }
    }
}