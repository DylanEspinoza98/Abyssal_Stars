using UnityEngine;

public class BossController : MonoBehaviour
{
    //  Movimiento del boss
    [Header("Movement")]
    [SerializeField] private float _moveSpeed = 2f;

    
    [SerializeField] private Vector2 _zoneCenter = new Vector2(0f, 2.5f);
    [SerializeField] private Vector2 _zoneSize = new Vector2(4f, 1.5f);

    // RECORDAR: movimiento en zig zag
    [SerializeField] private float _zigzagAmplitude = 1.5f;
    [SerializeField] private float _zigzagFrequency = 1.5f;

    // Movimiento circular 
    [SerializeField] private float _circleRadius = 1.2f;
    [SerializeField] private float _circleSpeed = 1.2f;

    private float _timeAccum = 0f;
    private bool _useCircle = false;   

    // ── Disparo del boss
    [Header("Shooting")]
    [SerializeField] private EnemyBullet _bulletPrefab;
    [SerializeField] private float _bulletSpeed = 6f;

    
    [SerializeField] private int _spreadCount = 5;
    [SerializeField] private float _spreadAngle = 60f;

    // Patron de rafaga
    [SerializeField] private int _burstCount = 3;
    [SerializeField] private float _burstInterval = 0.12f;

    // Fase y segundos por patron
    [SerializeField] private float _phaseDuration = 4f;   

    private float _phaseTimer = 0f;
    private int _currentPhase = 0;   
    private bool _burstRunning = false;

    [Header("Health")]
    [SerializeField] private float _health = 10f;

    public void TakeDamage(float amount)
    {
        _health -= amount;
        Debug.Log($"Boss golpeado! Vida restante: {_health}");

        if (_health <= 0)
            Die();
    }

    private void Die()
    {
        Debug.Log("Boss destruido!");
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

        HandleShooting();
    }

    

    private void HandleMovement()
    {
        Vector2 pos;

        if (_useCircle)
        {
            // Movimiento circular cerca del centro de zona
            float angle = _timeAccum * _circleSpeed;
            pos = _zoneCenter + new Vector2(
                Mathf.Cos(angle) * _circleRadius,
                Mathf.Sin(angle) * _circleRadius * 0.5f   
            );
        }
        else
        {
            
            float x = Mathf.Sin(_timeAccum * _zigzagFrequency) * _zigzagAmplitude;
            float y = Mathf.Sin(_timeAccum * _zigzagFrequency * 2f) * (_zoneSize.y * 0.3f);
            pos = _zoneCenter + new Vector2(x, y);
        }

        
        pos.x = Mathf.Clamp(pos.x, _zoneCenter.x - _zoneSize.x / 2f, _zoneCenter.x + _zoneSize.x / 2f);
        pos.y = Mathf.Clamp(pos.y, _zoneCenter.y - _zoneSize.y / 2f, _zoneCenter.y + _zoneSize.y / 2f);

        transform.position = Vector2.MoveTowards(transform.position, pos, _moveSpeed * Time.deltaTime);
    }

    // disparo

    private void HandleShooting()
    {
        if (_burstRunning) return;

        switch (_currentPhase)
        {
            case 0: 
                if (_phaseTimer % 1f < Time.deltaTime)
                    FireSpread();
                break;

            case 1: 
                if (_phaseTimer % 1.5f < Time.deltaTime)
                    StartCoroutine(FireBurst());
                break;

            case 2: 
                if (_phaseTimer % 0.8f < Time.deltaTime)
                    FireSpread();
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
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(_zoneCenter, _zoneSize);
    }
}