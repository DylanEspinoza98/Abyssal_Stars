using UnityEngine;
using System.Collections;

public class Familiar : MonoBehaviour
{
    [Header("Orbita")]
    [SerializeField] private float _orbitRadius = 1.2f;
    [SerializeField] private float _orbitSpeed = 90f;

    [Header("Disparo")]
    [SerializeField] private PlayerBullet _bulletPrefab;
    [SerializeField] private float _fireRate = 0.3f;
    [SerializeField] private float _bulletSpeed = 12f;
    [SerializeField] private Color _bulletColor = Color.cyan;

    private float _currentAngle = 0f;
    private float _fireTimer = 0f;
    private Transform _player;

    public void Init(Transform player, PlayerBullet bulletPrefab, float angleOffset)
    {
        _player = player;
        _bulletPrefab = bulletPrefab;
        _currentAngle = angleOffset;
    }

    void Update()
    {
        if (_player == null) return;

        _currentAngle += _orbitSpeed * Time.deltaTime;
        if (_currentAngle >= 360f) _currentAngle -= 360f;

        float rad = _currentAngle * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * _orbitRadius;
        transform.position = _player.position + offset;

        _fireTimer -= Time.deltaTime;
        if (_fireTimer <= 0f)
        {
            Shoot();
            _fireTimer = _fireRate;
        }
    }

    private void Shoot()
    {
        if (BulletPool.Instance == null || _bulletPrefab == null) return;

        PlayerBullet bullet = BulletPool.Instance.GetBullet(
            _bulletPrefab,
            transform.position,
            Quaternion.identity,
            Vector2.up * _bulletSpeed
        );

        SpriteRenderer sr = bullet.GetComponent<SpriteRenderer>();
        if (sr != null) sr.color = _bulletColor;
    }

    private void OnDestroy()
    {
        // API actualizada: sin FindObjectsSortMode
        PlayerBullet[] todas = FindObjectsByType<PlayerBullet>(FindObjectsInactive.Include);
        foreach (PlayerBullet b in todas)
        {
            SpriteRenderer sr = b.GetComponent<SpriteRenderer>();
            if (sr != null) sr.color = Color.white;
        }
    }
}