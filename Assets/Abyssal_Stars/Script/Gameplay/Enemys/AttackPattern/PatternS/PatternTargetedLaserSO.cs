using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "New Targeted Laser", menuName = "Boss Patterns/Attack/Targeted Laser")]
public class PatternTargetedLaserSO : AttackPatternSO
{
    [Header("Daño y Raycast")]
    [Tooltip("Vidas que quita por cada impacto.")]
    public int damageAmount = 1;
    [Tooltip("Cuántas veces por segundo quita vidas si el jugador se queda en el láser.")]
    public float damageRate = 4f;
    public float laserRange = 50f;
    [Tooltip("Asegúrate de que aquí esté seleccionada al menos la capa (Layer) del Player.")]
    public LayerMask whatToHit;

    [Header("Tiempos (Fases)")]
    public float aimTime = 1.5f;
    public float lockTime = 0.8f;
    public float fireDuration = 4.0f;
    public float overheatTime = 2.5f;

    [Header("Rotación")]
    public bool rotateWholeBody = false;
    public float rotationOffset = 90f;
    public float rotationSpeed = 5f;

    [Header("Visuales — Láser")]
    public float normalWidth = 0.05f;
    public float flashWidth = 0.4f;
    public Color aimColor = new Color(1f, 1f, 1f, 0.4f);
    public Color lockColor = new Color(1f, 0f, 1f, 1f);
    public Color fireColor = new Color(1f, 0f, 0f, 1f);

    [Header("Visuales — Sobrecalentamiento")]
    public Color overheatColor = new Color(1f, 0.4f, 0f, 1f);

    public override IEnumerator ExecutePattern(BossTurret turret)
    {
        if (turret.laserLineRenderer == null) yield break;

        Transform originPoint = turret.laserFirePoint != null ? turret.laserFirePoint : turret.transform;
        Transform targetToRotate = turret.transform;

        if (rotateWholeBody)
        {
            EnemyBase body = turret.GetComponentInParent<EnemyBase>();
            if (body != null) targetToRotate = body.transform;
        }

        while (true)
        {
            SetLaser(turret.laserLineRenderer, aimColor, normalWidth);
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

                DrawLaser(turret.laserLineRenderer, originPoint.position, originPoint.up);
                aimTimer += Time.deltaTime;
                yield return null;
            }

            Vector3 frozenDirection = originPoint.up;
            SetLaser(turret.laserLineRenderer, lockColor, normalWidth);
            float lockTimer = 0f;
            while (lockTimer < lockTime)
            {
                DrawLaser(turret.laserLineRenderer, originPoint.position, frozenDirection);
                lockTimer += Time.deltaTime;
                yield return null;
            }

            SetLaser(turret.laserLineRenderer, fireColor, flashWidth);
            float fireTimer = 0f;

            float damageInterval = damageRate > 0f ? 1f / damageRate : 1f;
            float nextAllowedDamageTime = 0f;

            float laserRadius = flashWidth / 2f;

            while (fireTimer < fireDuration)
            {
                DrawLaser(turret.laserLineRenderer, originPoint.position, frozenDirection);

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

            turret.laserLineRenderer.enabled = false;

            if (turret.turretSpriteRenderer != null)
            {
                Color originalColor = turret.turretSpriteRenderer.color;
                turret.turretSpriteRenderer.color = overheatColor;
                yield return new WaitForSeconds(overheatTime);
                if (turret.turretSpriteRenderer != null) turret.turretSpriteRenderer.color = originalColor;
            }
            else
            {
                yield return new WaitForSeconds(overheatTime);
            }
        }
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