using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "New SniperLaser", menuName = "Boss Patterns/Attack/Sniper")]
public class PatternSniperSO : AttackPatternSO
{
    [Header("Da�o y Raycast")]
    [Tooltip("Vidas que quita por cada impacto.")]
    [SerializeField] private int damageAmount = 1;
    [Tooltip("Cu�ntas veces por segundo quita vida si el jugador est� en el rayo.")]
    [SerializeField] private float damageRate = 4f;
    [Tooltip("Largo del rayo. Suficiente para cruzar toda la pantalla.")]
    [SerializeField] private float laserRange = 50f;
    [Tooltip("Debe incluir la capa del Player, igual que en PatternTargetedLaserSO.")]
    [SerializeField] private LayerMask whatToHit;

    [Header("Tiempos")]
    [Tooltip("Duraci�n del aviso visual antes de disparar.")]
    [SerializeField] private float telegraphDuration = 1.2f;
    [Tooltip("Duraci�n del disparo activo con da�o.")]
    [SerializeField] private float fireDuration = 2f;
    [Tooltip("Pausa entre ciclos.")]
    [SerializeField] private float cooldown = 2f;

    [Header("Visuales - Telegr�fico")]
    [Tooltip("Ancho del rayo de aviso.")]
    [SerializeField] private float telegraphWidth = 0.05f;
    [SerializeField] private Color telegraphColor = new Color(1f, 0.9f, 0f, 0.5f);

    [Header("Visuales - Disparo")]
    [Tooltip("Ancho del rayo activo. El CircleCast usa este valor como radio.")]
    [SerializeField] private float fireWidth = 0.35f;
    [SerializeField] private Color fireColor = new Color(1f, 0.1f, 0f, 1f);

    public override IEnumerator ExecutePattern(BossTurret turret)
    {
        if (turret.LaserLineRenderer == null) yield break;

        LineRenderer laser = turret.LaserLineRenderer;
        Transform firePoint = turret.LaserFirePoint != null
                                    ? turret.LaserFirePoint
                                    : turret.transform;

        while (true)
        {
            if (!turret || !turret.gameObject.activeInHierarchy)
            {
                if (laser != null) laser.enabled = false;
                yield break;
            }

            bool isRightSide = turret.transform.position.x > 0;
            float fireAngle = isRightSide ? 180f : 0f;
            float rad = fireAngle * Mathf.Deg2Rad;
            Vector3 fireDir = new Vector3(Mathf.Cos(rad), 0f, 0f);

            turret.RotateToAngle(fireAngle);

            SetLaser(laser, telegraphColor, telegraphWidth);

            float telegraphTimer = 0f;
            while (telegraphTimer < telegraphDuration)
            {
                if (!turret || !turret.gameObject.activeInHierarchy)
                {
                    laser.enabled = false;
                    yield break;
                }

                laser.SetPosition(0, firePoint.position);
                laser.SetPosition(1, firePoint.position + fireDir * laserRange);

                telegraphTimer += Time.deltaTime;
                yield return null;
            }

            SetLaser(laser, fireColor, fireWidth);

            float fireTimer = 0f;
            float damageInterval = damageRate > 0f ? 1f / damageRate : 1f;
            float nextDamageTime = 0f;
            float laserRadius = fireWidth / 2f;

            while (fireTimer < fireDuration)
            {
                if (!turret || !turret.gameObject.activeInHierarchy)
                {
                    laser.enabled = false;
                    yield break;
                }

                laser.SetPosition(0, firePoint.position);
                laser.SetPosition(1, firePoint.position + fireDir * laserRange);

                RaycastHit2D[] hits = Physics2D.CircleCastAll(
                    firePoint.position, laserRadius, fireDir, laserRange, whatToHit
                );

                foreach (RaycastHit2D hit in hits)
                {
                    if (hit.collider == null || !hit.collider.CompareTag("Player")) continue;
                    if (Time.time < nextDamageTime) continue;

                    PlayerHealth player = hit.collider.GetComponent<PlayerHealth>();
                    if (player == null) player = hit.collider.GetComponentInParent<PlayerHealth>();
                    if (player == null) player = hit.collider.GetComponentInChildren<PlayerHealth>();

                    if (player != null)
                    {
                        player.TakeDamage(damageAmount, false);
                        nextDamageTime = Time.time + damageInterval;
                    }
                }

                fireTimer += Time.deltaTime;
                yield return null;
            }

            laser.enabled = false;

            yield return new WaitForSeconds(cooldown);
        }
    }

    private void SetLaser(LineRenderer lr, Color color, float width)
    {
        lr.enabled = true;
        lr.positionCount = 2;
        lr.startColor = color;
        lr.endColor = color;
        lr.startWidth = width;
        lr.endWidth = width;
    }
}