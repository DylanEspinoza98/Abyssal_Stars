using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "New Static Aim", menuName = "Boss Patterns/Movement/Static Aim")]
public class MovementStaticAimSO : MovementPatternSO
{
    [Header("Ilusión de Apuntado (Anclado)")]
    [Tooltip("Si es True, rota suavemente hacia el jugador sin moverse del sitio.")]
    [SerializeField] private bool lookAtPlayer = true;
    [SerializeField] private float rotationSpeed = 5f;

    [Tooltip("Ajuste para alinear el frente de tu sprite (ej. 90).")]
    [SerializeField] private float rotationOffset = 90f;

    public override IEnumerator ExecuteMovement(Transform bossTransform, Vector2 zoneCenter)
    {
        while (true)
        {
            if (lookAtPlayer && PlayerHealth.Instance != null)
            {
                Vector3 direction = (PlayerHealth.Instance.transform.position - bossTransform.position).normalized;
                float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

                Quaternion targetRotation = Quaternion.Euler(0f, 0f, targetAngle + rotationOffset);

                bossTransform.rotation = Quaternion.Lerp(
                    bossTransform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
            }

            yield return null;
        }
    }
}