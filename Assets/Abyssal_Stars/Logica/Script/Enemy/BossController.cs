using UnityEngine;

public class BossController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float _moveSpeed = 2f;

    // Ahora estos valores son RELATIVOS al centro de la cámara
    [SerializeField] private Vector2 _zoneCenter = new Vector2(0f, 2.5f);
    [SerializeField] private Vector2 _zoneSize = new Vector2(4f, 1.5f);

    [SerializeField] private float _zigzagAmplitude = 1.5f;
    [SerializeField] private float _zigzagFrequency = 1.5f;

    [SerializeField] private float _circleRadius = 1.2f;
    [SerializeField] private float _circleSpeed = 1.2f;

    private float _timeAccum = 0f;
    private bool _useCircle = false;

    [Header("Shooting")]
    [SerializeField] private EnemyBullet _bulletPrefab;
    [SerializeField] private float _bulletSpeed = 6f;
    [SerializeField] private int _spreadCount = 5;
    [SerializeField] private float _spreadAngle = 60f;
    [SerializeField] private int _burstCount = 3;
    [SerializeField] private float _burstInterval = 0.12f;

    [Header("Phase Settings")]
    [SerializeField] private float _phaseDuration = 4f;

    private float _phaseTimer = 0f;
    private int _currentPhase = 0;
    private bool _burstRunning = false;

    [Header("Health")]
    [SerializeField] private float _health = 100f; // Subí la vida porque es un Boss

    private bool _isEntering = true;
    void Start()
    {
        if (transform.parent == null)
            transform.SetParent(Camera.main.transform);

        // Forzamos Z en 180 para que mire abajo
        // Mantenemos X e Y en 0 para que no se vea de lado o estirado
        transform.localRotation = Quaternion.Euler(0, 0, 180f);

        // Aseguramos la Z local para que no se esconda tras el fondo
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, 10f);
    }

    public void TakeDamage(float amount)
    {
        _health -= amount;
        Debug.Log($"Boss golpeado! Vida restante: {_health}");
        if (_health <= 0) Die();
    }

    private void Die()
    {
        Debug.Log("Boss destruido!");

        // Llamamos al Manager para que muestre la pantalla de Game Over (o Win en este caso)
        if (GameOverManager.Instance != null)
        {
            GameOverManager.Instance.ShowGameOver();
        }

        gameObject.SetActive(false);
    }

    void Update()
    {

        _timeAccum += Time.deltaTime;
        _phaseTimer += Time.deltaTime;

        HandleMovement();

        if (_phaseTimer >= _phaseDuration)
        {
            _phaseTimer = 0f;
            _currentPhase = (_currentPhase + 1) % 3;
            _useCircle = (_currentPhase == 2);
        }

        HandleMovement();
        HandleShooting();

        // BLOQUEO DE ROTACIÓN: Pase lo que pase, mira hacia abajo
        transform.localRotation = Quaternion.Euler(0, 0, 180f);

    }

    private void HandleMovement()
    {
        if (_isEntering)
        {
            // FASE DE ENTRADA: Baja lentamente hasta el centro de la zona
            transform.localPosition = Vector3.MoveTowards(
                transform.localPosition,
                new Vector3(_zoneCenter.x, _zoneCenter.y, 10f),
                _moveSpeed * Time.deltaTime
            );

            // Si ya llegó cerca del centro, activamos el combate
            if (Vector3.Distance(transform.localPosition, new Vector3(_zoneCenter.x, _zoneCenter.y, 10f)) < 0.1f)
            {
                _isEntering = false;
            }
            return; // No ejecutamos el zigzag mientras entra
        }

        // FASE DE COMBATE: Aquí va tu lógica de zigzag y círculos
        Vector3 targetLocalPos;
        if (_useCircle)
        {
            float angle = _timeAccum * _circleSpeed;
            targetLocalPos = new Vector3(
                _zoneCenter.x + Mathf.Cos(angle) * _circleRadius,
                _zoneCenter.y + Mathf.Sin(angle) * _circleRadius * 0.5f,
                10f
            );
        }
        else
        {
            float x = Mathf.Sin(_timeAccum * _zigzagFrequency) * _zigzagAmplitude;
            float y = Mathf.Sin(_timeAccum * _zigzagFrequency * 2f) * (_zoneSize.y * 0.3f);
            targetLocalPos = new Vector3(_zoneCenter.x + x, _zoneCenter.y + y, 10f);
        }

        transform.localPosition = Vector3.MoveTowards(transform.localPosition, targetLocalPos, _moveSpeed * Time.deltaTime);
    }

    private void HandleShooting()
    {
        if (_burstRunning) return;

        switch (_currentPhase)
        {
            case 0:
                if (_phaseTimer % 1f < Time.deltaTime) FireSpread();
                break;
            case 1:
                if (_phaseTimer % 1.5f < Time.deltaTime) StartCoroutine(FireBurst());
                break;
            case 2:
                if (_phaseTimer % 0.8f < Time.deltaTime) FireSpread();
                break;
        }
    }

    private void FireSpread()
    {
        float startAngle = -_spreadAngle / 2f;
        float step = _spreadAngle / (_spreadCount - 1);

        for (int i = 0; i < _spreadCount; i++)
        {
            float angle = startAngle + step * i;
            Vector2 dir = Rotate(Vector2.down, angle);
            SpawnBullet(dir);
        }
    }

    private System.Collections.IEnumerator FireBurst()
    {
        _burstRunning = true;
        for (int i = 0; i < _burstCount; i++)
        {
            SpawnBullet(Vector2.down);
            yield return new WaitForSeconds(_burstInterval);
        }
        _burstRunning = false;
    }

    private void SpawnBullet(Vector2 direction)
    {
        // La bala se spawnea en la posición del mundo (transform.position) 
        // pero la dirección y lógica ya están listas
        Quaternion rot = Quaternion.FromToRotation(Vector2.up, -direction);
        BulletPool.Instance.GetBullet(_bulletPrefab, transform.position, rot, direction * _bulletSpeed);
    }

    private Vector2 Rotate(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        return new Vector2(
            v.x * Mathf.Cos(rad) - v.y * Mathf.Sin(rad),
            v.x * Mathf.Sin(rad) + v.y * Mathf.Cos(rad)
        );
    }

    private void OnDrawGizmosSelected()
    {
        // Dibujamos el Gizmo relativo al padre (la cámara) para ver la zona en el editor
        if (transform.parent != null)
        {
            Gizmos.matrix = transform.parent.localToWorldMatrix;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(_zoneCenter, _zoneSize);
        }
    }
}