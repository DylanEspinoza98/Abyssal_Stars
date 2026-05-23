using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "New Zigzag", menuName = "Boss Patterns/Movement/Zigzag")]
public class MovementZigzagSO : MovementPatternSO
{
    [Header("Configuración")]
    [Tooltip("Amplitud horizontal del zigzag.")]
    public float amplitude = 1.5f;

    [Tooltip("Velocidad de la oscilación.")]
    public float frequency = 1.5f;

    [Tooltip("Qué tan rápido el boss sigue la posición objetivo.")]
    public float moveSpeed = 2f;

    [Tooltip("Variación vertical (porcentaje del zoneSize.y).")]
    [Range(0f, 1f)]
    public float verticalRatio = 0.3f;

    public override IEnumerator ExecuteMovement(Transform bossTransform, Vector2 zoneCenter)
    {
        float timeAccum = 0f;

        while (true)
        {
            timeAccum += Time.deltaTime;

            float x = Mathf.Sin(timeAccum * frequency) * amplitude;
            float y = Mathf.Sin(timeAccum * frequency * 2f) * verticalRatio;

            Vector3 target = new Vector3(zoneCenter.x + x, zoneCenter.y + y, 10f);
            bossTransform.localPosition = Vector3.MoveTowards(
                bossTransform.localPosition, target, moveSpeed * Time.deltaTime
            );

            yield return null;
        }
    }
}