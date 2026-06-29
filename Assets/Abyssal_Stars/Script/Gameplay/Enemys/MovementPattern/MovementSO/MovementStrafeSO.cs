using UnityEngine;
using System.Collections;


[CreateAssetMenu(fileName = "New Strafe", menuName = "Boss Patterns/Movement/Strafe")]
public class MovementStrafeSO : MovementPatternSO
{
    [Header("Movimiento Base")]
    [SerializeField] private float floatAmplitude = 0.2f;
    [SerializeField] private float floatSpeed     = 1f;
    [SerializeField] private float moveSpeed      = 1.5f;

    [Header("Strafe")]
    [Tooltip("Radio de detección de balas.")]
    [SerializeField] private float detectionRadius = 2.5f;

    [Tooltip("Tag de las balas del jugador.")]
    [SerializeField] private string playerBulletTag = "PlayerBullet";

    [Tooltip("Velocidad del strafe lateral.")]
    [SerializeField] private float strafeSpeed = 6f;

    [Tooltip("Distancia del desplazamiento lateral.")]
    [SerializeField] private float strafeDistance = 2f;

    [Tooltip("Límite máximo de desplazamiento en X desde el centro.")]
    [SerializeField] private float maxOffsetX = 3.5f;

    [Tooltip("Tiempo mínimo entre strafes (cooldown).")]
    [SerializeField] private float strafeCooldown = 0.6f;

    public override IEnumerator ExecuteMovement(Transform bossTransform, Vector2 zoneCenter)
    {
        float timeAccum    = 0f;
        float cooldownLeft = 0f;
        Vector3 basePos    = new Vector3(zoneCenter.x, zoneCenter.y, bossTransform.localPosition.z);
        Vector3 strafeTarget = bossTransform.localPosition;

        while (true)
        {
            timeAccum    += Time.deltaTime;
            cooldownLeft -= Time.deltaTime;

            // Detectar bala más cercana y esquivar al lado contrario
            if (cooldownLeft <= 0f)
            {
                Vector2 bulletDir = GetNearestBulletDirection(bossTransform.position);
                if (bulletDir != Vector2.zero)
                {
                    // Esquivar al lado opuesto de donde viene la bala en X
                    float dodgeX = -Mathf.Sign(bulletDir.x) * strafeDistance;
                    float newX   = Mathf.Clamp(
                        bossTransform.localPosition.x + dodgeX,
                        zoneCenter.x - maxOffsetX,
                        zoneCenter.x + maxOffsetX
                    );

                    strafeTarget  = new Vector3(newX, basePos.y, basePos.z);
                    cooldownLeft  = strafeCooldown;
                }
            }

            // Mover hacia el objetivo de strafe con flotación vertical
            float offsetY  = Mathf.Sin(timeAccum * floatSpeed) * floatAmplitude;
            Vector3 target = new Vector3(strafeTarget.x, basePos.y + offsetY, basePos.z);

            bossTransform.localPosition = Vector3.MoveTowards(
                bossTransform.localPosition, target, strafeSpeed * Time.deltaTime
            );

            yield return null;
        }
    }
    private Vector2 GetNearestBulletDirection(Vector3 bossWorldPos)
    {
        Collider2D[] hits    = Physics2D.OverlapCircleAll(bossWorldPos, detectionRadius);
        float        minDist = float.MaxValue;
        Vector2      dir     = Vector2.zero;

        foreach (Collider2D hit in hits)
        {
            if (!hit.CompareTag(playerBulletTag)) continue;

            float dist = Vector2.Distance(bossWorldPos, hit.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                dir     = ((Vector2)(hit.transform.position - bossWorldPos)).normalized;
            }
        }

        return dir;
    }
}
