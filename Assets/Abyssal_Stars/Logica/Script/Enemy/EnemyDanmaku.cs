using UnityEngine;
using System.Collections;

public class EnemyDanmaku : EnemyBase
{
    [Header("Ajustes Danmaku (Bullet Hell)")]
    [SerializeField] private Bullet _bulletPrefab;
    [SerializeField] private float _bulletSpeed = 4f;
    [SerializeField] private int _bulletsPerRing = 12;
    [SerializeField] private float _timeBetweenRings = 0.3f;
    [SerializeField] private float _angleOffsetPerRing = 15f;

    [Header("Movimiento")]
    [SerializeField] private float _entrySpeed = 3f;
    private Vector3 _targetPosition;
    private bool _isFiring = false;
    private float _currentAngleOffset = 0f;

    protected override void OnEnable()
    {
        base.OnEnable();
        _isFiring = false;
        _currentAngleOffset = 0f;

        // CORRECCIÓN: Ahora baja 3 unidades respecto a donde aparece y se detiene
        _targetPosition = transform.localPosition + new Vector3(0, -3f, 0);
    }

    protected override void Update()
    {
        base.Update();

        if (!_isFiring)
        {
            // MoveTowards asegura que avance sin pasarse de largo
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, _targetPosition, _entrySpeed * Time.deltaTime);

            // Cuando la distancia es casi cero, se detiene y desata el infierno
            if (Vector3.Distance(transform.localPosition, _targetPosition) < 0.01f)
            {
                _isFiring = true;
                StartCoroutine(FireDanmakuPattern());
            }
        }
    }

    private IEnumerator FireDanmakuPattern()
    {
        while (gameObject.activeInHierarchy)
        {
            float angleStep = 360f / _bulletsPerRing;

            for (int i = 0; i < _bulletsPerRing; i++)
            {
                float currentAngle = (i * angleStep) + _currentAngleOffset;
                Quaternion rot = Quaternion.Euler(0, 0, currentAngle);
                Vector2 dir = rot * Vector2.down;

                BulletPool.Instance.GetBullet(_bulletPrefab, transform.position, rot, dir * _bulletSpeed);
            }

            _currentAngleOffset += _angleOffsetPerRing;
            if (_currentAngleOffset >= 360f) _currentAngleOffset -= 360f;

            yield return new WaitForSeconds(_timeBetweenRings);
        }
    }
}