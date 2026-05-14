using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

[RequireComponent(typeof(PlayerHealth))]
public class PlayerShooter : MonoBehaviour
{
    [Header("Disparo Normal")]
    [SerializeField] private PlayerBullet _bulletPrefab;
    [SerializeField] private float _fireRate = 0.15f;
    [SerializeField] private float _bulletSpeed = 12f;

    [Header("Modo Shotgun")]
    [SerializeField] private int _shotgunBullets = 5;       // cantidad de perdigones
    [SerializeField] private float _shotgunSpread = 30f;    // angulo total del abanico en grados
    [SerializeField] private float _shotgunDuration = 10f;  // duracion del power up
    [SerializeField] private float _shotgunFireRate = 0.25f;// cadencia del shotgun (un poco mas lenta)

    private float _fireTimer;
    private bool _isShotgunActive = false;
    private Coroutine _shotgunCoroutine;
    private PlayerHealth health;

    void Start()
    {
        health = GetComponent<PlayerHealth>();
    }

    void Update()
    {
        if (health != null && health.IsDead) return;
        HandleShooting();
    }

    private void HandleShooting()
    {
        if (_fireTimer > 0) _fireTimer -= Time.deltaTime;

        if (Keyboard.current != null && Keyboard.current.spaceKey.isPressed && _fireTimer <= 0)
        {
            if (_isShotgunActive)
                ShootShotgun();
            else
                ShootNormal();

            _fireTimer = _isShotgunActive ? _shotgunFireRate : _fireRate;
        }
    }

    // Disparo normal: 1 bala hacia arriba
    private void ShootNormal()
    {
        Vector2 velocity = transform.up * _bulletSpeed;
        BulletPool.Instance.GetBullet(_bulletPrefab, transform.position, transform.rotation, velocity);
    }

    // Disparo shotgun: abanico de balas
    private void ShootShotgun()
    {
        // Calcula el angulo de cada perdigon distribuido en el abanico
        float halfSpread = _shotgunSpread / 2f;
        float angleStep = _shotgunBullets > 1 ? _shotgunSpread / (_shotgunBullets - 1) : 0f;

        for (int i = 0; i < _shotgunBullets; i++)
        {
            float angle = -halfSpread + (angleStep * i);
            Quaternion spreadRotation = Quaternion.Euler(0, 0, angle);
            Vector2 dir = spreadRotation * transform.up;
            Quaternion bulletRot = Quaternion.Euler(0, 0, angle) * transform.rotation;

            BulletPool.Instance.GetBullet(_bulletPrefab, transform.position, bulletRot, dir * _bulletSpeed);
        }
    }

    // Llamado por PowerUpShotgun al recogerlo
    public void ActivateShotgun()
    {
        // Si ya habia uno activo, lo cancela y reinicia el timer
        if (_shotgunCoroutine != null)
            StopCoroutine(_shotgunCoroutine);

        _shotgunCoroutine = StartCoroutine(ShotgunRoutine());
    }

    private IEnumerator ShotgunRoutine()
    {
        _isShotgunActive = true;
        yield return new WaitForSeconds(_shotgunDuration);
        _isShotgunActive = false;
        _shotgunCoroutine = null;
    }

    // Util para saber desde UI si el shotgun esta activo
    public bool IsShotgunActive => _isShotgunActive;
}