using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerHealth))]
public class PlayerShooter : MonoBehaviour
{
    [Header("Shooting Settings")]
    [SerializeField] private PlayerBullet _bulletPrefab;
    [SerializeField] private float _fireRate = 0.15f;
    [SerializeField] private float _bulletSpeed = 12f;

    private float _fireTimer;
    private PlayerHealth health;

    void Start()
    {
        health = GetComponent<PlayerHealth>();
    }

    void Update()
    {
        // Si está muerto, no puede disparar
        if (health != null && health.IsDead) return;

        HandleShooting();
    }

    private void HandleShooting()
    {
        if (_fireTimer > 0) _fireTimer -= Time.deltaTime;
        if (Keyboard.current != null && Keyboard.current.spaceKey.isPressed && _fireTimer <= 0)
        {
            Shoot();
            _fireTimer = _fireRate;
        }
    }

    private void Shoot()
    {
        Vector2 velocity = transform.up * _bulletSpeed;
        BulletPool.Instance.GetBullet(_bulletPrefab, transform.position, transform.rotation, velocity);
    }
}