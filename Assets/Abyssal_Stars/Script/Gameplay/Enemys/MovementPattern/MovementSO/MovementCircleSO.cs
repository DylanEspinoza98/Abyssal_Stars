using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "New Circle", menuName = "Boss Patterns/Movement/Circle")]
public class MovementCircleSO : MovementPatternSO
{
    [Header("Entrada Dinámica")]
    [Tooltip("Centro = 0.5. Arriba = 0.85 a 1.0.")]
    public Vector2 viewportTarget = new Vector2(0.5f, 0.85f);
    [SerializeField] private float entrySpeed = 4f;

    [Header("Órbita")]
    [SerializeField] private float radius = 1.2f;
    [Range(0.1f, 1f)] [SerializeField] private float verticalRatio = 0.5f;
    [SerializeField] private float orbitSpeed = 1.2f;
    [SerializeField] private float moveSpeed = 2f;

    [Tooltip("Desfase inicial en radianes. PI/2 (1.5708) fuerza el inicio en el centro del eje X.")]
    [SerializeField] private float initialAngleOffset = Mathf.PI / 2f;

    public override IEnumerator ExecuteMovement(Transform bossTransform, Vector2 zoneCenter)
    {
        float startZ = bossTransform.position.z;
        Vector3 orbitCenter = CalculateViewportWorldPosition(startZ);

        while (Vector3.Distance(bossTransform.position, orbitCenter) > 0.01f)
        {
            bossTransform.position = Vector3.MoveTowards(
                bossTransform.position, orbitCenter, entrySpeed * Time.deltaTime
            );
            yield return null;
        }

        float timeAccum = 0f;
        while (true)
        {
            timeAccum += Time.deltaTime;

            float currentAngle = (timeAccum * orbitSpeed) + initialAngleOffset;

            float targetX = orbitCenter.x + Mathf.Cos(currentAngle) * radius;
            float targetY = orbitCenter.y + Mathf.Sin(currentAngle) * radius * verticalRatio;

            Vector3 targetPosition = new Vector3(targetX, targetY, startZ);

            bossTransform.position = Vector3.MoveTowards(
                bossTransform.position, targetPosition, moveSpeed * Time.deltaTime
            );

            yield return null;
        }
    }

    private Vector3 CalculateViewportWorldPosition(float zDepth)
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("[MovementCircleSO] No se encontró Camera.main. Retornando Vector3.zero.");
            return Vector3.zero;
        }

        Vector3 worldPos = cam.ViewportToWorldPoint(new Vector3(viewportTarget.x, viewportTarget.y, cam.nearClipPlane));
        return new Vector3(worldPos.x, worldPos.y, zDepth);
    }
}