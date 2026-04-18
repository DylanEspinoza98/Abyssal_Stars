using UnityEngine;
using System.Collections; 

public class BossController : EnemyBase
{
    [Header("Movement")]
    [SerializeField] private float _moveSpeed = 2f;
    [SerializeField] private Vector2 _zoneCenter = new Vector2(0f, 2.5f);
    [SerializeField] private Vector2 _zoneSize = new Vector2(4f, 1.5f);
    [SerializeField] private float _zigzagAmplitude = 1.5f;
    [SerializeField] private float _zigzagFrequency = 1.5f;
    [SerializeField] private float _circleRadius = 1.2f;
    [SerializeField] private float _circleSpeed = 1.2f;

    [Header("Shooting")]
    [SerializeField] private Bullet _bulletPrefab;
    [SerializeField] private float _bulletSpeed = 6f;
    [SerializeField] private int _spreadCount = 5;
    [SerializeField] private float _spreadAngle = 60f;
    [SerializeField] private int _burstCount = 3;
    [SerializeField] private float _burstInterval = 0.12f;

    [Header("Phase Settings")]
    [SerializeField] private float _phaseDuration = 4f;

    private float _timeAccum = 0f;
    private bool _useCircle = false;
    private float _phaseTimer = 0f;
    private int _currentPhase = 0;
    private bool _burstRunning = false;
    private bool _isEntering = true;

    private bool _isDying = false;

    protected override void OnEnable()
    {
        base.OnEnable();

        if (transform.parent == null && Camera.main != null)
            transform.SetParent(Camera.main.transform);

        transform.localRotation = Quaternion.Euler(0, 0, 180f);
        _isEntering = true;
        _phaseTimer = 0;
        _currentPhase = 0;
        _isDying = false; 
    }

    protected override void Update()
    {
        if (_isDying) return;

        _timeAccum += Time.deltaTime;
        _phaseTimer += Time.deltaTime;

        HandleMovement();
        HandleShooting();
        UpdatePhases();
    }

    private void HandleMovement()
    {
        if (_isEntering)
        {
            Vector3 entryPos = new Vector3(_zoneCenter.x, _zoneCenter.y, 10f);
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, entryPos, _moveSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.localPosition, entryPos) < 0.1f)
                _isEntering = false;

            return;
        }

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

    private void UpdatePhases()
    {
        if (_phaseTimer >= _phaseDuration)
        {
            _phaseTimer = 0;
            _currentPhase = (_currentPhase + 1) % 3;
            _useCircle = !_useCircle;
        }
    }

    private void HandleShooting()
    {
        if (_isEntering || _burstRunning) return;

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

    private IEnumerator FireBurst()
    {
        _burstRunning = true;
        for (int i = 0; i < _burstCount; i++)
        {
            // Seguro por si muere a mitad de ráfaga
            if (_isDying || !gameObject.activeInHierarchy) yield break;

            SpawnBullet(Vector2.down);
            yield return new WaitForSeconds(_burstInterval);
        }
        _burstRunning = false;
    }

    private void SpawnBullet(Vector2 direction)
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion bulletRotation = Quaternion.Euler(0, 0, angle - 90f);

        if (BulletPool.Instance != null)
        {
            BulletPool.Instance.GetBullet(_bulletPrefab, transform.position, bulletRotation, direction * _bulletSpeed);
        }
    }

    private Vector2 Rotate(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        return new Vector2(
            v.x * Mathf.Cos(rad) - v.y * Mathf.Sin(rad),
            v.x * Mathf.Sin(rad) + v.y * Mathf.Cos(rad)
        );
    }
    protected override void Die()
    {
        if (_isDying) return;

        StartCoroutine(TheatricalDeathRoutine());
    }

    private IEnumerator TheatricalDeathRoutine()
    {
        _isDying = true; 

        if (AudioBeatDetector.Instance != null)
        {
            AudioBeatDetector.Instance.StopMusic();
        }

        int explosionCount = 6;
        for (int i = 0; i < explosionCount; i++)
        {
            if (_explosionEffectPrefab != null)
            {
                Vector3 randomOffset = new Vector3(Random.Range(-2f, 2f), Random.Range(-1.5f, 1.5f), 0);
                Instantiate(_explosionEffectPrefab, transform.position + randomOffset, Quaternion.identity);
            }

            yield return new WaitForSeconds(0.4f - (i * 0.05f));
        }

        if (GameOverManager.Instance != null)
        {
            GameOverManager.Instance.ShowGameOver();
        }

        base.Die();
    }
}