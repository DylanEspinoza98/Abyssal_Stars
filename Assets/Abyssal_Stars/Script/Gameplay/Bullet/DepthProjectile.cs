using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class DepthProjectile : EnemyBullet
{
    [Header("Efecto 3D (Z-Axis Falso)")]
    [Tooltip("Segundos que tarda la bala en viajar del fondo a la pantalla.")]
    [SerializeField] private float _timeToForeground = 1.5f;
    [SerializeField] private float _startScale = 0.2f;
    [SerializeField] private float _endScale = 1f;

    [Header("Configuración Visual")]
    [SerializeField] private int _backgroundSortingOrder = -15;
    [SerializeField] private int _foregroundSortingOrder = 5;

    public float TimeToForeground => _timeToForeground;

    private Collider2D _col;
    private Coroutine _emergeCoroutine;

    // --- VARIABLES DE GRID MATEMÁTICO ---
    private bool _isGridManaged = false;
    private Vector2 _gridSpawnPos;
    private Vector2 _gridForwardDir;
    private float _gridForwardSpeed;
    private Vector2 _gridLocalOffset;
    private float _gridRotationSpeed;
    private float _aliveTime;

    protected override void Awake()
    {
        base.Awake();
        _col = GetComponent<Collider2D>();
    }

    // -------------------------------------------------------------------------
    // NUEVO MÉTODO CENTRAL: Configura la bala para ser parte de un enjambre que gira
    // -------------------------------------------------------------------------
    public void FireHexGrid(Vector2 spawnPos, Vector2 forwardDir, float forwardSpeed, Vector2 localOffset, float rotationSpeed)
    {
        Fire(forwardDir, forwardSpeed); // Inicializa rotación del sprite y lógica base

        _isGridManaged = true;
        _gridSpawnPos = spawnPos;
        _gridForwardDir = forwardDir;
        _gridForwardSpeed = forwardSpeed;
        _gridLocalOffset = localOffset;
        _gridRotationSpeed = rotationSpeed;
        _aliveTime = 0f;

        if (_emergeCoroutine != null) StopCoroutine(_emergeCoroutine);
        _emergeCoroutine = StartCoroutine(EmergeVisualsOnlyRoutine());
    }

    // -------------------------------------------------------------------------
    // LA MAGIA ORBITAL: Calculamos la posición exacta en cada frame
    // -------------------------------------------------------------------------
    protected override void Update()
    {
        if (_isGridManaged)
        {
            _aliveTime += Time.deltaTime;

            // 1. Centro dinámico: ¿Dónde está el centro del hexágono ahora mismo?
            Vector2 center = _gridSpawnPos + _gridForwardDir * (_gridForwardSpeed * _aliveTime);

            // 2. Expansión (Clamp01 evita que siga creciendo después del tiempo límite)
            float t = Mathf.Clamp01(_aliveTime / _timeToForeground);
            float smoothT = t * t * t;

            // 3. Rotación continua sobre el eje
            float currentAngle = _gridRotationSpeed * _aliveTime;
            Quaternion rot = Quaternion.Euler(0, 0, currentAngle);
            Vector2 rotatedOffset = rot * _gridLocalOffset;

            // 4. Posición Final = Centro + (Offset * Factor de Expansión)
            transform.position = center + (rotatedOffset * smoothT);

            // 5. Truco clave: Anulamos la Velocity base para que Bullet.cs no mueva la bala dos veces
            Velocity = Vector2.zero;
        }

        // Llamamos al Update base de Bullet.cs. Como Velocity es 0, no la moverá,
        // pero sí chequeará si salió de la pantalla para destruirla.
        base.Update();
    }

    // -------------------------------------------------------------------------
    // Corrutina puramente visual (Escala y Transparencia)
    // -------------------------------------------------------------------------
    private IEnumerator EmergeVisualsOnlyRoutine()
    {
        if (_col != null) _col.enabled = false;
        if (_spriteRenderer != null) _spriteRenderer.sortingOrder = _backgroundSortingOrder;

        transform.localScale = Vector3.one * _startScale;
        float elapsed = 0f;
        Vector3 finalScale = Vector3.one * _endScale;

        while (elapsed < _timeToForeground)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / _timeToForeground;
            float smoothT = t * t * t;

            transform.localScale = Vector3.Lerp(Vector3.one * _startScale, finalScale, smoothT);

            if (_spriteRenderer != null)
            {
                Color c = _spriteRenderer.color;
                c.a = Mathf.Lerp(0.3f, 1f, smoothT);
                _spriteRenderer.color = c;
            }

            yield return null;
        }

        MakeForeground(finalScale);
    }

    private void MakeForeground(Vector3 finalScale)
    {
        transform.localScale = finalScale;
        if (_col != null) _col.enabled = true;

        if (_spriteRenderer != null)
        {
            _spriteRenderer.sortingOrder = _foregroundSortingOrder;
            Color c = _spriteRenderer.color;
            c.a = 1f;
            _spriteRenderer.color = c;
        }
    }

    public override void ResetBullet()
    {
        base.ResetBullet();
        _isGridManaged = false;

        if (_emergeCoroutine != null)
        {
            StopCoroutine(_emergeCoroutine);
            _emergeCoroutine = null;
        }

        transform.localScale = Vector3.one * _endScale;
        if (_col != null) _col.enabled = true;
    }
}