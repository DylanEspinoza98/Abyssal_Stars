using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "New Targeted Laser", menuName = "Boss Patterns/Attack/Targeted Laser")]
public class PatternTargetedLaserSO : AttackPatternSO
{
    [Header("Da�o y Raycast")]
    [Tooltip("Vidas que quita por cada impacto.")]
    [SerializeField] private int damageAmount = 1;
    [Tooltip("Cu�ntas veces por segundo quita vidas si el jugador se queda en el l�ser.")]
    [SerializeField] private float damageRate = 4f;
    [SerializeField] private float laserRange = 50f;
    [Tooltip("Aseg�rate de que aqu� est� seleccionada al menos la capa (Layer) del Player.")]
    [SerializeField] private LayerMask whatToHit;

    [Header("Tiempos (Fases)")]
    [SerializeField] private float aimTime = 1.5f;
    [SerializeField] private float lockTime = 0.8f;
    [SerializeField] private float fireDuration = 4.0f;
    [SerializeField] private float overheatTime = 2.5f;

    [Header("Rotaci�n")]
    [SerializeField] private bool rotateWholeBody = false;
    [SerializeField] private float rotationOffset = 90f;
    [SerializeField] private float rotationSpeed = 5f;

    [Header("Visuales � L�ser")]
    [SerializeField] private float normalWidth = 0.05f;
    [SerializeField] private float flashWidth = 0.4f;
    [SerializeField] private Color aimColor = new Color(1f, 1f, 1f, 0.4f);
    [SerializeField] private Color lockColor = new Color(1f, 0f, 1f, 1f);
    [SerializeField] private Color fireColor = new Color(1f, 0f, 0f, 1f);

    [Header("Visuales � Sobrecalentamiento")]
    [SerializeField] private Color overheatColor = new Color(1f, 0.4f, 0f, 1f);

    public override IEnumerator ExecutePattern(BossTurret turret)
    {
        if (turret.LaserLineRenderer == null) yield break;

        Transform originPoint = turret.LaserFirePoint != null ? turret.LaserFirePoint : turret.transform;
        Transform targetToRotate = turret.transform;

        if (rotateWholeBody)
        {
            EnemyBase body = turret.GetComponentInParent<EnemyBase>();
            if (body != null) targetToRotate = body.transform;
        }

        // Este patrón controla la rotación manualmente mediante Lerp.
        // Desactivar el sistema automático de BossTurret para evitar conflicto.
        turret.EnableAutoRotation(false);

        while (true)
        {
            SetLaser(turret.LaserLineRenderer, aimColor, normalWidth);
            float aimTimer = 0f;
            while (aimTimer < aimTime)
            {
                if (PlayerHealth.Instance != null)
                {
                    Vector2 dir = (PlayerHealth.Instance.transform.position - targetToRotate.position).normalized;
                    float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                    Quaternion targetRot = Quaternion.Euler(0f, 0f, ApplyMirror(angle) + rotationOffset);
                    targetToRotate.rotation = Quaternion.Lerp(targetToRotate.rotation, targetRot, rotationSpeed * Time.deltaTime);
                }

                DrawLaser(turret.LaserLineRenderer, originPoint.position, originPoint.up);
                aimTimer += Time.deltaTime;
                yield return null;
            }

            Vector3 frozenDirection = originPoint.up;
            SetLaser(turret.LaserLineRenderer, lockColor, normalWidth);
            float lockTimer = 0f;
            while (lockTimer < lockTime)
            {
                DrawLaser(turret.LaserLineRenderer, originPoint.position, frozenDirection);
                lockTimer += Time.deltaTime;
                yield return null;
            }

            SetLaser(turret.LaserLineRenderer, fireColor, flashWidth);
            float fireTimer = 0f;

            float damageInterval = damageRate > 0f ? 1f / damageRate : 1f;
            float nextAllowedDamageTime = 0f;

            float laserRadius = flashWidth / 2f;

            while (fireTimer < fireDuration)
            {
                DrawLaser(turret.LaserLineRenderer, originPoint.position, frozenDirection);

                RaycastHit2D[] hits = Physics2D.CircleCastAll(originPoint.position, laserRadius, frozenDirection, laserRange, whatToHit);

                foreach (RaycastHit2D hit in hits)
                {
                    if (hit.collider != null && hit.collider.CompareTag("Player"))
                    {
                        if (Time.time >= nextAllowedDamageTime)
                        {
                            PlayerHealth player = hit.collider.GetComponent<PlayerHealth>();

                            if (player == null) player = hit.collider.GetComponentInParent<PlayerHealth>();
                            if (player == null) player = hit.collider.GetComponentInChildren<PlayerHealth>();

                            if (player != null)
                            {
                                player.TakeDamage(damageAmount, false);

                                nextAllowedDamageTime = Time.time + damageInterval;
                            }
                        }
                    }
                }

                fireTimer += Time.deltaTime;
                yield return null;
            }

            turret.LaserLineRenderer.enabled = false;

            if (turret.TurretSpriteRenderer != null)
            {
                Color originalColor = turret.TurretSpriteRenderer.color;
                turret.TurretSpriteRenderer.color = overheatColor;
                yield return new WaitForSeconds(overheatTime);
                if (turret.TurretSpriteRenderer != null) turret.TurretSpriteRenderer.color = originalColor;
            }
            else
            {
                yield return new WaitForSeconds(overheatTime);
            }
        }
    }

    public override void OnStopped(BossTurret turret)
    {
        // Restaurar el sistema automático al detenerse el patrón.
        turret.EnableAutoRotation(true);
        if (turret.LaserLineRenderer != null)
            turret.LaserLineRenderer.enabled = false;
    }

    private void DrawLaser(LineRenderer lr, Vector3 origin, Vector3 direction)
    {
        lr.SetPosition(0, origin);
        lr.SetPosition(1, origin + direction * laserRange);
    }

    private void SetLaser(LineRenderer lr, Color color, float width)
    {
        lr.enabled = true;
        lr.startColor = color;
        lr.endColor = color;
        lr.startWidth = width;
        lr.endWidth = width;
    }
}