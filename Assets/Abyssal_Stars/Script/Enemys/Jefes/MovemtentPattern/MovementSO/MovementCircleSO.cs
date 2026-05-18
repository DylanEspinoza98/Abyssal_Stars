using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "New Circle", menuName = "Boss Patterns/Movement/Circle")]
public class MovementCircleSO : MovementPatternSO
{
    [Header("Configuración")]
    [Tooltip("Radio horizontal de la órbita.")]
    public float radius = 1.2f;

    [Tooltip("Radio vertical (0.5 = elipse achatada, 1 = círculo perfecto).")]
    [Range(0.1f, 1f)]
    public float verticalRatio = 0.5f;

    [Tooltip("Velocidad angular (radianes/segundo).")]
    public float orbitSpeed = 1.2f;

    [Tooltip("Qué tan rápido el boss sigue la posición objetivo.")]
    public float moveSpeed = 2f;

    public override IEnumerator ExecuteMovement(Transform bossTransform, Vector2 zoneCenter)
    {
        float timeAccum = 0f;

        while (true)
        {
            timeAccum += Time.deltaTime;

            float angle = timeAccum * orbitSpeed;
            float x = zoneCenter.x + Mathf.Cos(angle) * radius;
            float y = zoneCenter.y + Mathf.Sin(angle) * radius * verticalRatio;

            Vector3 target = new Vector3(x, y, 10f);
            bossTransform.localPosition = Vector3.MoveTowards(
                bossTransform.localPosition, target, moveSpeed * Time.deltaTime
            );

            yield return null;
        }
    }
}