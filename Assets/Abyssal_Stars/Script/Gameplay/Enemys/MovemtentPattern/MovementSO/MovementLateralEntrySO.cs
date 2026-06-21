using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "New Lateral Entry", menuName = "Enemy Patterns/Movement/Lateral Entry")]
public class MovementLateralEntrySO : MovementPatternSO
{
    [Header("Deslizamiento al Lateral")]
    [Tooltip("Velocidad con la que se mueve desde donde entró hasta el anclaje lateral.")]
    public float slideSpeed = 5f;

    [Header("Posición de Anclaje")]
    [Tooltip("Distancia desde el borde de pantalla en viewport. " +
             "El lado se detecta automáticamente según la zona de spawn (X positivo = derecha).")]
    [Range(0.05f, 0.35f)]
    public float anchorViewportX = 0.1f;

    [Tooltip("Altura mínima del anclaje en viewport (0 = abajo).")]
    [Range(0f, 1f)]
    public float anchorViewportYMin = 0.2f;

    [Tooltip("Altura máxima del anclaje en viewport (1 = arriba).")]
    [Range(0f, 1f)]
    public float anchorViewportYMax = 0.8f;

    [Header("Flotación en el Anclaje")]
    [Tooltip("Amplitud del bob vertical una vez anclado.")]
    public float bobAmplitude = 0.15f;

    [Tooltip("Velocidad del bob vertical.")]
    public float bobSpeed = 1.5f;

    public override IEnumerator ExecuteMovement(Transform enemyTransform, Vector2 zoneCenter)
    {
        Camera cam = Camera.main;
        if (cam == null) yield break;

        bool isRightSide = zoneCenter.x > 0;

        float anchorVpX = isRightSide ? (1f - anchorViewportX) : anchorViewportX;
        float anchorVpY = Random.Range(anchorViewportYMin, anchorViewportYMax);

        Vector3 anchorWorld = cam.ViewportToWorldPoint(
            new Vector3(anchorVpX, anchorVpY, cam.nearClipPlane)
        );
        anchorWorld.z = enemyTransform.position.z;

        while (Vector3.Distance(enemyTransform.position, anchorWorld) > 0.05f)
        {
            enemyTransform.position = Vector3.MoveTowards(
                enemyTransform.position,
                anchorWorld,
                slideSpeed * Time.deltaTime
            );
            yield return null;
        }

        enemyTransform.position = anchorWorld;

        float timeAccum = 0f;
        while (true)
        {
            timeAccum += Time.deltaTime;
            float offsetY = Mathf.Sin(timeAccum * bobSpeed) * bobAmplitude;

            enemyTransform.position = new Vector3(
                anchorWorld.x,
                anchorWorld.y + offsetY,
                anchorWorld.z
            );

            yield return null;
        }
    }
}