using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "New MovementPendulumFallSO Movement", menuName = "Boss Patterns/Movement/MovementPendulumFallSO")]
public class MovementPendulumFallSO : MovementPatternSO
{
    [Header("Ajustes de Descenso")]
    [SerializeField] private float fallSpeed = 1.5f;

    [Header("Oscilación Horizontal")]
    [SerializeField] private float horizontalSpeed = 3f;
    [SerializeField] private float horizontalLimit = 4.2f;

    public override IEnumerator ExecuteMovement(Transform bossTransform, Vector2 zoneCenter)
    {
        float directionX = 1f;

        while (true)
        {
            bossTransform.Translate(Vector3.down * fallSpeed * Time.deltaTime, Space.World);
            bossTransform.Translate(Vector3.right * directionX * horizontalSpeed * Time.deltaTime, Space.World);

            if (bossTransform.position.x >= zoneCenter.x + horizontalLimit)
            {
                directionX = -1f;
            }
            else if (bossTransform.position.x <= zoneCenter.x - horizontalLimit)
            {
                directionX = 1f;
            }

            yield return null;
        }
    }
}