using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "New Dash Attack", menuName = "Boss Patterns/Movement/Dash Attack")]
public class MovementDashAttackSO : MovementPatternSO
{
    [Header("Fase 1: Merodeo (Izquierda a Derecha)")]
    [Tooltip("Velocidad a la que se mueve de lado a lado.")]
    [SerializeField] private float hoverSpeed = 2f;
    [Tooltip("Qué tan lejos llega hacia los lados.")]
    [SerializeField] private float hoverAmplitude = 4f;
    [Tooltip("Cuánto tiempo merodea antes de decidir atacar.")]
    [SerializeField] private float hoverDuration = 3f;

    [Header("Fase 2: Advertencia (Vibración)")]
    [SerializeField] private float shakeDuration = 0.8f;
    [SerializeField] private float shakeIntensity = 0.2f;

    [Header("Fase 3: El Ataque (Dash)")]
    [Tooltip("Velocidad de la embestida hacia abajo.")]
    [SerializeField] private float dashSpeed = 25f;
    [Tooltip("Hasta qué coordenada Y baja el jefe para golpear.")]
    [SerializeField] private float dashBottomY = -6f;

    [Header("Fase 4: Regreso")]
    [Tooltip("Velocidad para volver a su posición de vuelo normal.")]
    [SerializeField] private float returnSpeed = 10f;
    [Tooltip("Tiempo de descanso antes de volver a moverse de lado a lado.")]
    [SerializeField] private float restDuration = 1f;

    public override IEnumerator ExecuteMovement(Transform bossTransform, Vector2 zoneCenter)
    {
        float globalTimer = 0f;

        while (true)
        {
            float hoverTimer = 0f;
            while (hoverTimer < hoverDuration)
            {
                hoverTimer += Time.deltaTime;
                globalTimer += Time.deltaTime;

                float offsetX = Mathf.Sin(globalTimer * hoverSpeed) * hoverAmplitude;
                bossTransform.localPosition = new Vector3(zoneCenter.x + offsetX, zoneCenter.y, bossTransform.localPosition.z);

                yield return null;
            }

            Vector3 preDashPos = bossTransform.localPosition;

            float shakeTimer = 0f;
            while (shakeTimer < shakeDuration)
            {
                shakeTimer += Time.deltaTime;
                Vector2 randomShake = Random.insideUnitCircle * shakeIntensity;
                bossTransform.localPosition = preDashPos + (Vector3)randomShake;
                yield return null;
            }

            bossTransform.localPosition = preDashPos;

            Vector3 dashTarget = new Vector3(preDashPos.x, dashBottomY, preDashPos.z);

            while (Vector3.Distance(bossTransform.localPosition, dashTarget) > 0.1f)
            {
                bossTransform.localPosition = Vector3.MoveTowards(bossTransform.localPosition, dashTarget, dashSpeed * Time.deltaTime);
                yield return null;
            }

            bossTransform.localPosition = dashTarget;
            yield return new WaitForSeconds(0.2f);

            Vector3 returnTarget = new Vector3(preDashPos.x, zoneCenter.y, preDashPos.z);

            while (Vector3.Distance(bossTransform.localPosition, returnTarget) > 0.1f)
            {
                bossTransform.localPosition = Vector3.MoveTowards(bossTransform.localPosition, returnTarget, returnSpeed * Time.deltaTime);
                yield return null;
            }

            bossTransform.localPosition = returnTarget;

            // Descanso antes de repetir el ciclo
            yield return new WaitForSeconds(restDuration);
        }
    }
    public override void OnStopped(Transform bossTransform) {}
}