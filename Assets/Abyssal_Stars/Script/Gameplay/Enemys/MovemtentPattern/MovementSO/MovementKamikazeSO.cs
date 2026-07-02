using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "New Kamikaze", menuName = "Enemy Patterns/Movement/Kamikaze")]
public class MovementKamikazeSO : MovementPatternSO
{
    [Header("Configuración")]
    [Tooltip("Velocidad del lanzamiento.")]
    public float diveSpeed = 14f;

    [Tooltip("Si está activo, apunta hacia el jugador al lanzarse. Si no, va directo hacia abajo.")]
    public bool aimAtPlayer = true;

    [Tooltip("Tag del jugador (solo si aimAtPlayer está activo).")]
    public string playerTag = "Player";

    [Header("Rotación Visual")]
    [Tooltip("Ajuste en grados para corregir el frente del sprite. Usa 90 si tu imagen original mira hacia arriba.")]
    public float rotationOffset = 90f;

    public override IEnumerator ExecuteMovement(Transform enemyTransform, Vector2 zoneCenter)
    {
        Vector3 direction = Vector3.down;

        if (aimAtPlayer)
        {
            GameObject player = GameObject.FindWithTag(playerTag);

            if (player != null)
            {
                direction = (player.transform.position - enemyTransform.position).normalized;

                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                enemyTransform.rotation = Quaternion.Euler(0f, 0f, angle + rotationOffset);
            }
        }

        while (true)
        {
            enemyTransform.Translate(direction * diveSpeed * Time.deltaTime, Space.World);
            yield return null;
        }
    }
}