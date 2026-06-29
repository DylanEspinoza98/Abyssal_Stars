using UnityEngine;

public class SpawnZone : MonoBehaviour
{
    [Header("Zona de Spawn")]
    [Tooltip("Mitad del ancho de la zona.")]
    [SerializeField] private float _rangeX = 2f;
    public float RangeX => _rangeX;

    [Tooltip("Mitad del alto de la zona.")]
    [SerializeField] private float _rangeY = 0.5f;
    public float RangeY => _rangeY;

    [Header("Debug")]
    [SerializeField] private Color _gizmoColor = new Color(0.2f, 0.8f, 1f, 0.35f);

    public Vector3 GetRandomPosition()
    {
        float x = transform.position.x + Random.Range(-_rangeX, _rangeX);
        float y = transform.position.y + Random.Range(-_rangeY, _rangeY);
        return new Vector3(x, y, transform.position.z);
    }

    // Gizmos 

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = _gizmoColor;
        Gizmos.DrawWireCube(transform.position, new Vector3(_rangeX * 2f, _rangeY * 2f, 0f));

        Color fill = _gizmoColor;
        fill.a = 0.08f;
        Gizmos.color = fill;
        Gizmos.DrawCube(transform.position, new Vector3(_rangeX * 2f, _rangeY * 2f, 0f));
    }

    private void OnDrawGizmosSelected()
    {
        Color selected = _gizmoColor;
        selected.a = 0.9f;
        Gizmos.color = selected;
        Gizmos.DrawWireCube(transform.position, new Vector3(_rangeX * 2f, _rangeY * 2f, 0f));

        UnityEditor.Handles.color = selected;
        UnityEditor.Handles.Label(
            transform.position + Vector3.up * (_rangeY + 0.25f),
            gameObject.name
        );
    }
#endif
}