using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "Nueva Espiral", menuName = "Boss Patterns/Espiral")]
public class PatternSpiralSO : AttackPatternSO
{
    [Header("Configuración")]
    public float fireRate = 0.08f;
    public float bulletSpeed = 5f;
    public int arms = 3;
    public float spinSpeed = 15f;

    public override IEnumerator ExecutePattern(BossTurret turret)
    {
        float currentAngle = 0f;

        while (true)
        {
            float angleStep = 360f / arms;

            for (int i = 0; i < arms; i++)
            {
                float finalAngle = currentAngle + (angleStep * i);
                turret.FireSingleBullet(finalAngle, bulletSpeed);
            }

            currentAngle += spinSpeed;

            yield return new WaitForSeconds(fireRate);
        }
    }
}