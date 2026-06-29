using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "New Retreat Vertical", menuName = "Boss Patterns/Movement/Retreat Vertical")]
public class MovementRetreatSO : MovementPatternSO
{
    [Header("Movimiento Base")]
    [Tooltip("Flotación suave cuando no hay amenaza.")]
    [SerializeField] private float floatAmplitude = 0.3f;
    [SerializeField] private float floatSpeed     = 1.2f;
    [SerializeField] private float moveSpeed      = 1.5f;

    [Header("Retirada")]
    [Tooltip("Radio de detección de balas del jugador.")]
    [SerializeField] private float detectionRadius = 3f;

    [Tooltip("Tag de las balas del jugador.")]
    [SerializeField] private string playerBulletTag = "PlayerBullet";

    [Tooltip("Velocidad de retirada hacia arriba.")]
    [SerializeField] private float retreatSpeed = 5f;

    [Tooltip("Cuánto sube al retirarse (en unidades locales).")]
    [SerializeField] private float retreatAmount = 1.5f;

    [Tooltip("Tiempo que mantiene la posición retirada antes de volver.")]
    [SerializeField] private float retreatHoldTime = 1.2f;

    public override IEnumerator ExecuteMovement(Transform bossTransform, Vector2 zoneCenter)
    {
        float timeAccum    = 0f;
        bool  isRetreating = false;
        float retreatTimer = 0f;

        Vector3 basePos   = new Vector3(zoneCenter.x, zoneCenter.y, bossTransform.localPosition.z);
        Vector3 retreatPos = basePos + Vector3.up * retreatAmount;

        while (true)
        {
            timeAccum += Time.deltaTime;

            bool bulletNearby = DetectPlayerBullets(bossTransform.position);

            if (bulletNearby && !isRetreating)
            {
                isRetreating = true;
                retreatTimer = retreatHoldTime;
            }

            if (isRetreating)
            {
                retreatTimer -= Time.deltaTime;

                bossTransform.localPosition = Vector3.MoveTowards(
                    bossTransform.localPosition, retreatPos, retreatSpeed * Time.deltaTime
                );

                if (retreatTimer <= 0f)
                    isRetreating = false;
            }
            else
            {
                // Flotación suave en la posición base
                float offsetY  = Mathf.Sin(timeAccum * floatSpeed) * floatAmplitude;
                Vector3 target = new Vector3(basePos.x, basePos.y + offsetY, basePos.z);

                bossTransform.localPosition = Vector3.MoveTowards(
                    bossTransform.localPosition, target, moveSpeed * Time.deltaTime
                );
            }

            yield return null;
        }
    }

    private bool DetectPlayerBullets(Vector3 worldPosition)
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(worldPosition, detectionRadius);
        foreach (Collider2D hit in hits)
            if (hit.CompareTag(playerBulletTag)) return true;
        return false;
    }
}
