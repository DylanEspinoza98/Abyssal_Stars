using UnityEngine;
using System.Collections;

public class EnemySniper : EnemyBase
{
    [Header("Configuración de Ataque (Láser Continuo)")]
    [SerializeField] private int _damageAmount = 1;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private float _aimTime = 1.5f;
    [SerializeField] private float _lockTime = 0.8f;

    [Tooltip("Tiempo que el rayo letal se mantiene bloqueando el escenario")]
    [SerializeField] private float _fireDuration = 4.0f;
    [SerializeField] private LayerMask _whatToHit;

    [Header("Sobrecalentamiento")]
    [Tooltip("Tiempo que el enemigo se queda quieto y sin disparar tras atacar")]
    [SerializeField] private float _overheatTime = 2.5f;
    [SerializeField] private Color _overheatColor = new Color(1f, 0.4f, 0f, 1f);

    [Header("Referencia Visual (Láser)")]
    [SerializeField] private LineRenderer _lineRenderer;
    [SerializeField] private float _laserRange = 50f;
    [SerializeField] private float _normalWidth = 0.05f;
    [SerializeField] private float _flashWidth = 0.4f;
    [SerializeField] private Color _aimColor = new Color(1f, 1f, 1f, 0.4f);
    [SerializeField] private Color _lockColor = new Color(1f, 0f, 1f, 1f);
    [SerializeField] private Color _fireColor = new Color(1f, 0f, 0f, 1f);

    [Header("Movimiento de Entrada")]
    [SerializeField] private float _entrySpeed = 3f;

    private Vector3 _targetPosition;
    private bool _isPositioned = false;

    protected override void OnEnable()
    {
        base.OnEnable();
        _isPositioned = false;

        if (_lineRenderer != null) _lineRenderer.enabled = false;

        _targetPosition = transform.localPosition + new Vector3(0, -2.5f, 0);
    }

    protected override void Update()
    {
        base.Update();

        if (!_isPositioned && !_isRetreating)
        {
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, _targetPosition, _entrySpeed * Time.deltaTime);
            if (Vector3.Distance(transform.localPosition, _targetPosition) < 0.01f)
            {
                _isPositioned = true;
                StartCoroutine(SniperRoutine());
            }
        }
    }

    private IEnumerator SniperRoutine()
    {

        while (gameObject.activeInHierarchy && !_isRetreating)
        {
            // 1. FASE DE APUNTADO (Persigue al jugador)
            ConfigurarVisualLaser(_aimColor, _normalWidth);
            float aimTimer = 0f;
            while (aimTimer < _aimTime)
            {
                ActualizarApuntado(true);
                aimTimer += Time.deltaTime;
                yield return null;
            }

            // 2. FASE DE FIJADO (Se queda congelado advirtiendo dónde va a disparar)
            ConfigurarVisualLaser(_lockColor, _normalWidth);
            float lockTimer = 0f;
            while (lockTimer < _lockTime)
            {
                ActualizarApuntado(false);
                lockTimer += Time.deltaTime;
                yield return null;
            }

            // 3. FASE DE DISPARO SOSTENIDO (El Muro Láser)
            yield return StartCoroutine(EjecutarDisparoSostenido());

            // 4. FASE DE SOBRECALENTAMIENTO (Vulnerable e inactivo)
            yield return StartCoroutine(SobrecalentamientoRoutine());
        }
    }

    private void ActualizarApuntado(bool debeRotar)
    {
        if (debeRotar && PlayerHealth.Instance != null)
        {
            Vector2 direccion = (PlayerHealth.Instance.transform.position - transform.position).normalized;
            float angulo = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angulo - 90f);
        }

        if (_lineRenderer != null && _firePoint != null)
        {
            _lineRenderer.SetPosition(0, _firePoint.position);
            _lineRenderer.SetPosition(1, _firePoint.position + (transform.up * _laserRange));
        }
    }

    private IEnumerator EjecutarDisparoSostenido()
    {
        ConfigurarVisualLaser(_fireColor, _flashWidth);

        float fireTimer = 0f;

        while (fireTimer < _fireDuration && !_isRetreating)
        {
            ActualizarApuntado(false);

            RaycastHit2D hit = Physics2D.Raycast(_firePoint.position, transform.up, _laserRange, _whatToHit);

            if (hit.collider != null && hit.collider.CompareTag("Player"))
            {
                PlayerHealth player = hit.collider.GetComponent<PlayerHealth>();

                if (player != null && !player.IsInvincible)
                {
                    player.TakeDamage(_damageAmount);
                }
            }

            fireTimer += Time.deltaTime;
            yield return null;
        }

        if (_lineRenderer != null) _lineRenderer.enabled = false;
    }

    private IEnumerator SobrecalentamientoRoutine()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        Color originalColor = Color.white;

        if (sr != null)
        {
            originalColor = sr.color;
            sr.color = _overheatColor; 
        }

        yield return new WaitForSeconds(_overheatTime);

        if (sr != null)
        {
            sr.color = originalColor;
        }
    }

    private void ConfigurarVisualLaser(Color col, float ancho)
    {
        if (_lineRenderer != null)
        {
            _lineRenderer.enabled = true;
            _lineRenderer.startColor = col;
            _lineRenderer.endColor = col;
            _lineRenderer.startWidth = ancho;
            _lineRenderer.endWidth = ancho;
        }
    }

    public override void ReturnToPool()
    {
        if (_lineRenderer != null) _lineRenderer.enabled = false;
        base.ReturnToPool();
    }
}