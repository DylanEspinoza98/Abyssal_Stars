using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "New Mirror", menuName = "Enemy Patterns/Movement/Mirror")]
public class MovementMirrorSO : MovementPatternSO
{
    [Header("Seguimiento Horizontal")]
    [Tooltip("Velocidad con la que sigue la X del jugador. Más bajo = más perezoso.")]
    public float trackingSpeed = 3f;

    [Tooltip("Límite horizontal máximo desde el centro de su zona.")]
    public float maxOffsetX = 4f;

    [Tooltip("Tag del jugador.")]
    public string playerTag = "Player";

    public override IEnumerator ExecuteMovement(Transform enemyTransform, Vector2 zoneCenter)
    {
        GameObject player = GameObject.FindWithTag(playerTag);

        while (true)
        {
            if (player != null)
            {
                float targetX = Mathf.Clamp(
                    player.transform.position.x,
                    zoneCenter.x - maxOffsetX,
                    zoneCenter.x + maxOffsetX
                );

                float newX = Mathf.Lerp(
                    enemyTransform.position.x,
                    targetX,
                    trackingSpeed * Time.deltaTime
                );

                enemyTransform.position = new Vector3(
                    newX,
                    zoneCenter.y,
                    enemyTransform.position.z
                );
            }

            yield return null;
        }
    }
}