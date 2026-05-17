using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "New Hover", menuName = "Boss Patterns/Movement/Hover")]
public class MovementHoverSO : MovementPatternSO
{
    [Header("Configuración")]
    [Tooltip("Amplitud de la vibración (muy pequeña para que sea sutil).")]
    public float vibrateAmplitude = 0.08f;

    [Tooltip("Velocidad de la vibración.")]
    public float vibrateSpeed = 8f;

    [Tooltip("Velocidad para llegar al centro al iniciar la fase.")]
    public float returnSpeed = 3f;

    public override IEnumerator ExecuteMovement(Transform bossTransform, Vector2 zoneCenter)
    {
        Vector3 center = new Vector3(zoneCenter.x, zoneCenter.y, bossTransform.localPosition.z);

        // Primero volver al centro suavemente
        while (Vector3.Distance(bossTransform.localPosition, center) > 0.05f)
        {
            bossTransform.localPosition = Vector3.MoveTowards(
                bossTransform.localPosition, center, returnSpeed * Time.deltaTime
            );
            yield return null;
        }

        // Hover: vibración sutil en X e Y
        float timeAccum = 0f;
        while (true)
        {
            timeAccum += Time.deltaTime;

            float offsetX = Mathf.Sin(timeAccum * vibrateSpeed)          * vibrateAmplitude;
            float offsetY = Mathf.Sin(timeAccum * vibrateSpeed * 1.3f)   * vibrateAmplitude;

            bossTransform.localPosition = new Vector3(
                center.x + offsetX,
                center.y + offsetY,
                center.z
            );

            yield return null;
        }
    }
}
