using UnityEngine;

public class SpawnZone : MonoBehaviour
{
    [Header("Zona de Spawn")]
    [Tooltip("Mitad del ancho de la zona.")]
    [SerializeField] public float rangeX = 2f;

    [Tooltip("Mitad del alto de la zona.")]
    [SerializeField] public float rangeY = 0.5f;

    [Header("Debug")]
    [SerializeField] private Color _gizmoColor = new Color(0.2f, 0.8f, 1f, 0.35f);

    public Vector3 GetRandomPosition()
    {
        float x = transform.position.x + Random.Range(-rangeX, rangeX);
        float y = transform.position.y + Random.Range(-rangeY, rangeY);
        return new Vector3(x, y, transform.position.z);
    }

    // Gizmos 

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = _gizmoColor;
        Gizmos.DrawWireCube(transform.position, new Vector3(rangeX * 2f, rangeY * 2f, 0f));

        Color fill = _gizmoColor;
        fill.a = 0.08f;
        Gizmos.color = fill;
        Gizmos.DrawCube(transform.position, new Vector3(rangeX * 2f, rangeY * 2f, 0f));
    }

    private void OnDrawGizmosSelected()
    {
        Color selected = _gizmoColor;
        selected.a = 0.9f;
        Gizmos.color = selected;
        Gizmos.DrawWireCube(transform.position, new Vector3(rangeX * 2f, rangeY * 2f, 0f));

        UnityEditor.Handles.color = selected;
        UnityEditor.Handles.Label(
            transform.position + Vector3.up * (rangeY + 0.25f),
            gameObject.name
        );
    }
#endif
}