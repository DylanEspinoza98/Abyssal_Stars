using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "New Dash", menuName = "Boss Patterns/Movement/Dash")]
public class MovementDashSO : MovementPatternSO
{
    [Header("Configuración")]
    [Tooltip("Velocidad del dash.")]
    public float dashSpeed = 12f;

    [Tooltip("Pausa entre dashes (segundos).")]
    public float pauseBetweenDashes = 0.8f;

    [Tooltip("Cuánto puede alejarse del centro en X.")]
    public float rangeX = 3f;

    [Tooltip("Cuánto puede alejarse del centro en Y.")]
    public float rangeY = 1f;

    [Tooltip("Distancia mínima entre el punto actual y el destino " +
             "(evita dashes muy cortos).")]
    public float minDistance = 1.5f;

    public override IEnumerator ExecuteMovement(Transform bossTransform, Vector2 zoneCenter)
    {
        while (true)
        {
            // Elegir destino aleatorio con distancia mínima
            Vector3 destination;
            int safety = 10;
            do
            {
                float x = zoneCenter.x + Random.Range(-rangeX, rangeX);
                float y = zoneCenter.y + Random.Range(-rangeY, rangeY);
                destination = new Vector3(x, y, bossTransform.localPosition.z);
                safety--;
            }
            while (Vector3.Distance(bossTransform.localPosition, destination) < minDistance && safety > 0);

            // Dash hacia el destino
            while (Vector3.Distance(bossTransform.localPosition, destination) > 0.05f)
            {
                bossTransform.localPosition = Vector3.MoveTowards(
                    bossTransform.localPosition, destination, dashSpeed * Time.deltaTime
                );
                yield return null;
            }

            // Pausa antes del siguiente dash
            yield return new WaitForSeconds(pauseBetweenDashes);
        }
    }
}
