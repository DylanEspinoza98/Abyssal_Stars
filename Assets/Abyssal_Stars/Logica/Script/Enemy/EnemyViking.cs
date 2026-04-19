using UnityEngine;
using System.Collections;

public class EnemyViking : EnemyBase
{
    [Header("Ajustes de Ataque")]
    [SerializeField] private Bullet _bulletPrefab;
    [SerializeField] private float _bulletSpeed = 8f;
    [SerializeField] private int _shotsPerBurst = 3;
    [SerializeField] private float _timeBetweenShots = 0.15f;
    [SerializeField] private float _attackRange = 10f;

    [Header("Ajustes de Movimiento")]
    [SerializeField] private float _entrySpeed = 4f;
    [SerializeField] private float _exitSpeed = 6f;

    [Header("Tiempos de Decisión")]
    [SerializeField] private float _maxPatience = 3f;
    [SerializeField] private float _spawnGracePeriod = 1.5f;
    private float _patienceTimer;

    private bool _hasFired = false;
    private bool _isExiting = false;
    private Vector3 _targetPosition;
    private Vector2 _exitDirection;

    protected override void OnEnable()
    {
        base.OnEnable();
        _hasFired = false;
        _isExiting = false;
        _patienceTimer = 0f;

        _targetPosition = transform.localPosition + new Vector3(0, -3f, 0);

        _exitDirection = new Vector2(transform.localPosition.x > 0 ? 1f : -1f, 0.5f).normalized;
    }

    protected override void Update()
    {
        if (_isExiting || _patienceTimer > _spawnGracePeriod)
        {
            base.Update();
        }

        if (_isExiting)
        {
            transform.Translate(_exitDirection * _exitSpeed * Time.deltaTime, Space.Self);
        }
        else
        {
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, _targetPosition, _entrySpeed * Time.deltaTime);

            _patienceTimer += Time.deltaTime;

            if (PlayerHealth.Instance != null && !_hasFired)
            {
                float distToPlayer = Vector2.Distance(transform.position, PlayerHealth.Instance.transform.position);

                if (distToPlayer <= _attackRange)
                {
                    StartCoroutine(FireBurst());
                }
                else if (_patienceTimer >= _maxPatience)
                {
                    _isExiting = true;
                }
            }
        }
    }

    private IEnumerator FireBurst()
    {
        _hasFired = true;

        for (int i = 0; i < _shotsPerBurst; i++)
        {
            if (!gameObject.activeInHierarchy) yield break;

            if (PlayerHealth.Instance != null)
            {
                Vector2 dir = (PlayerHealth.Instance.transform.position - transform.position).normalized;
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                Quaternion rot = Quaternion.Euler(0, 0, angle - 90f);

                BulletPool.Instance.GetBullet(_bulletPrefab, transform.position, rot, dir * _bulletSpeed);
            }
            yield return new WaitForSeconds(_timeBetweenShots);
        }

        _isExiting = true;
    }
}