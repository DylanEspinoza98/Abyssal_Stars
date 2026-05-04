using UnityEngine;
using System.Collections;

public class EnemySniper : EnemyBase
{
    [Header("Configuración de Ataque (Hitscan)")]
    [SerializeField] private float _damageAmount = 1f;
    [SerializeField] private Transform _firePoint;
    [SerializeField] private float _aimTime = 1.5f;  
    [SerializeField] private float _lockTime = 0.8f; 
    [SerializeField] private float _cooldown = 2f;
    [SerializeField] private LayerMask _whatToHit;

    [Header("Referencia Visual (Láser)")]
    [SerializeField] private LineRenderer _lineRenderer;
    [SerializeField] private float _laserRange = 50f;
    [SerializeField] private float _normalWidth = 0.05f;
    [SerializeField] private float _flashWidth = 0.25f;
    [SerializeField] private Color _aimColor = new Color(1f, 1f, 1f, 0.4f);
    [SerializeField] private Color _lockColor = new Color(1f, 0f, 1f, 1f); 

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
        if (!_isPositioned)
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
        while (gameObject.activeInHierarchy)
        {
            ConfigurarVisualLaser(_aimColor, _normalWidth);

            float aimTimer = 0f;
            while (aimTimer < _aimTime)
            {
                ActualizarApuntado(true);
                aimTimer += Time.deltaTime;
                yield return null;
            }

            ConfigurarVisualLaser(_lockColor, _normalWidth);

            float lockTimer = 0f;
            while (lockTimer < _lockTime)
            {
                ActualizarApuntado(false);
                lockTimer += Time.deltaTime;
                yield return null;
            }

            EjecutarDisparoHitscan();

            yield return new WaitForSeconds(_cooldown);
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

    private void EjecutarDisparoHitscan()
    {
        RaycastHit2D hit = Physics2D.Raycast(_firePoint.position, transform.up, _laserRange, _whatToHit);

        if (hit.collider != null && hit.collider.CompareTag("Player"))
        {
            hit.collider.SendMessage("TakeDamage", _damageAmount, SendMessageOptions.DontRequireReceiver);
        }

        StartCoroutine(FlashVisualDisparo());
    }

    private IEnumerator FlashVisualDisparo()
    {
        if (_lineRenderer != null)
        {
            _lineRenderer.startWidth = _flashWidth;
            _lineRenderer.endWidth = _flashWidth;
            yield return new WaitForSeconds(0.1f);
            _lineRenderer.enabled = false;
            _lineRenderer.startWidth = _normalWidth;
            _lineRenderer.endWidth = _normalWidth;
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