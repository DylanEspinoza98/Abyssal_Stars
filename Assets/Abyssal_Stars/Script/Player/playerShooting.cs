using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(PlayerHealth))]
public class PlayerShooter : MonoBehaviour
{
    [Header("Disparo Normal")]
    [SerializeField] private PlayerBullet _bulletPrefab;
    [SerializeField] private float _fireRate = 0.15f;
    [SerializeField] private float _bulletSpeed = 12f;

    [Header("Modo Shotgun")]
    [SerializeField] private int _shotgunBullets = 5;
    [SerializeField] private float _shotgunSpread = 30f;
    [SerializeField] private float _shotgunDuration = 10f;
    [SerializeField] private float _shotgunFireRate = 0.25f;

    [Header("Familiar")]
    [SerializeField] private Familiar _familiarPrefab;
    [SerializeField] private int _maxFamiliars = 3;

    private float _fireTimer;
    private bool _isShotgunActive = false;
    private Coroutine _shotgunCoroutine;
    private List<Familiar> _familiars = new List<Familiar>();
    private PlayerHealth _health;

    void Start()
    {
        _health = GetComponent<PlayerHealth>();
        // Suscribirse a cuando el jugador muere para destruir familiares
        _health.OnLivesChanged += OnLivesChanged;
    }

    void OnDestroy()
    {
        if (_health != null)
            _health.OnLivesChanged -= OnLivesChanged;
    }

    private void OnLivesChanged(int lives)
    {
        // Cada vez que el jugador pierde una vida destruye todos los familiares
        DestroyAllFamiliars();
    }

    void Update()
    {
        if (_health != null && _health.IsDead) return;
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

    private void ShootNormal()
    {
        Vector2 velocity = transform.up * _bulletSpeed;
        BulletPool.Instance.GetBullet(_bulletPrefab, transform.position, transform.rotation, velocity);
    }

    private void ShootShotgun()
    {
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

    public void ActivateShotgun()
    {
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

    public void ActivateFamiliar()
    {
        // Limpia familiares destruidos de la lista
        _familiars.RemoveAll(f => f == null);

        // Si ya tiene el maximo no hace nada
        if (_familiars.Count >= _maxFamiliars) return;

        if (_familiarPrefab == null) return;

        // Calcula el angulo inicial segun cuantos familiares ya hay
        // 1 familiar: 0°   2: 0° y 120°   3: 0°, 120° y 240°
        float angleOffset = _familiars.Count * (360f / _maxFamiliars);

        Familiar newFamiliar = Instantiate(_familiarPrefab, transform.position, Quaternion.identity);
        newFamiliar.Init(transform, _bulletPrefab, angleOffset);
        _familiars.Add(newFamiliar);
    }

    private void DestroyAllFamiliars()
    {
        foreach (Familiar f in _familiars)
        {
            if (f != null) Destroy(f.gameObject);
        }
        _familiars.Clear();
    }

    public bool IsShotgunActive => _isShotgunActive;
    public int FamiliarCount => _familiars.Count;
}