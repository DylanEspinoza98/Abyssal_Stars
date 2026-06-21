using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "New Aimed Spread", menuName = "Boss Patterns/Attack/Aimed Spread")]
public class PatternAimedSpreadSO : AttackPatternSO
{
    [Header("Disparo")]
    [Tooltip("Velocidad de las balas.")]
    public float bulletSpeed = 5f;

    [Tooltip("Cantidad de balas por salva.")]
    [Min(1)]
    public int bulletCount = 3;

    [Tooltip("Ángulo total del abanico en grados.")]
    [Range(10f, 120f)]
    public float spreadAngle = 40f;

    [Tooltip("Pausa entre cada salva (después del disparo).")]
    public float fireRate = 0.8f;

    [Header("Telegráfico")]
    [Tooltip("Tiempo que la torreta apunta al jugador antes de disparar. " +
             "Da al jugador una ventana para anticipar el disparo.")]
    public float telegraphDuration = 0.4f;

    [Tooltip("Tag del jugador.")]
    public string playerTag = "Player";

    public override IEnumerator ExecutePattern(BossTurret turret)
    {
        while (true)
        {
            GameObject player = GameObject.FindWithTag(playerTag);

            if (player != null)
            {
                float elapsed = 0f;
                while (elapsed < telegraphDuration)
                {
                    Vector2 telegraphDir = (player.transform.position - turret.transform.position).normalized;
                    float telegraphAngle = Mathf.Atan2(telegraphDir.y, telegraphDir.x) * Mathf.Rad2Deg;
                    turret.RotateToAngle(telegraphAngle);

                    elapsed += Time.deltaTime;
                    yield return null;
                }

                Vector2 fireDir = (player.transform.position - turret.transform.position).normalized;
                float centerAngle = Mathf.Atan2(fireDir.y, fireDir.x) * Mathf.Rad2Deg;

                float startAngle = centerAngle - spreadAngle * 0.5f;
                float step = bulletCount > 1 ? spreadAngle / (bulletCount - 1) : 0f;

                for (int i = 0; i < bulletCount; i++)
                    turret.FireSingleBullet(ApplyMirror(startAngle + step * i), bulletSpeed);
            }

            yield return new WaitForSeconds(fireRate);
        }
    }
}