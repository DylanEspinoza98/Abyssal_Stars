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
    [Tooltip("Sprite que se usa mientras la escopeta está activa.")]
    [SerializeField] private Sprite _shotgunSprite;

    [Header("Familiar")]
    [SerializeField] private Familiar _familiarPrefab;
    [SerializeField] private int _maxFamiliars = 3;

    private float _fireTimer;
    private bool _isShotgunActive = false;
    private Coroutine _shotgunCoroutine;
    private List<Familiar> _familiars = new List<Familiar>();
    private PlayerHealth _health;

    private SpriteRenderer _spriteRenderer;
    private Sprite _originalSprite;
    private Vector3 _originalScale;

    void Start()
    {
        _health = GetComponent<PlayerHealth>();
        _spriteRenderer = GetComponent<SpriteRenderer>();

        if (_spriteRenderer != null)
        {
            _originalSprite = _spriteRenderer.sprite;
            _originalScale = transform.localScale;
        }

        _health.OnPlayerDied += OnPlayerDied;
    }

    void OnDestroy()
    {
        if (_health != null)
            _health.OnPlayerDied -= OnPlayerDied;
    }

    private void OnPlayerDied()
    {
        DestroyAllFamiliars();
    }

    void Update()
    {
        if (Time.timeScale == 0f) return;

        if (_health != null && _health.IsDead) return;

        HandleShooting();
    }

    private void HandleShooting()
    {
        if (_fireTimer > 0) _fireTimer -= Time.deltaTime;

        if (Keyboard.current != null && DataManager.Instance != null)
        {
            SettingsData settings = DataManager.Instance.SaveData.settings;

            if (IsKeyPressed(settings.shootKey) && _fireTimer <= 0)
            {
                if (_isShotgunActive)
                    ShootShotgun();
                else
                    ShootNormal();

                _fireTimer = _isShotgunActive ? _shotgunFireRate : _fireRate;
            }
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

        if (_spriteRenderer != null && _shotgunSprite != null)
        {
            _spriteRenderer.sprite = _shotgunSprite;
            NormalizeSpriteSize();
        }

        yield return new WaitForSeconds(_shotgunDuration);

        if (_spriteRenderer != null && _originalSprite != null)
        {
            _spriteRenderer.sprite = _originalSprite;
            transform.localScale = _originalScale;
        }

        _isShotgunActive = false;
        _shotgunCoroutine = null;
    }

    private void NormalizeSpriteSize()
    {
        Vector2 originalSize = _originalSprite.bounds.size;
        Vector2 newSize = _shotgunSprite.bounds.size;

        if (newSize.x == 0f || newSize.y == 0f) return;

        float scaleX = originalSize.x / newSize.x;
        float scaleY = originalSize.y / newSize.y;

        transform.localScale = new Vector3(
            _originalScale.x * scaleX,
            _originalScale.y * scaleY,
            _originalScale.z
        );
    }

    public void ActivateFamiliar()
    {
        _familiars.RemoveAll(f => f == null);

        if (_familiars.Count >= _maxFamiliars)
        {
            if (ScoreManager.Instance != null) ScoreManager.Instance.AddScore(5000);

            foreach (Familiar f in _familiars)
            {
                if (f != null) f.ActivateOverdrive(5f);
            }
            return;
        }

        if (_familiarPrefab == null) return;

        Familiar newFamiliar = Instantiate(_familiarPrefab, transform.position, Quaternion.identity);
        newFamiliar.Init(transform, _bulletPrefab, 0f);
        _familiars.Add(newFamiliar);

        float baseAngle = _familiars[0]._currentAngle;

        for (int i = 0; i < _familiars.Count; i++)
        {
            float perfectSpacing = 360f / _familiars.Count;

            _familiars[i]._currentAngle = baseAngle + (i * perfectSpacing);
        }
    }

    private void DestroyAllFamiliars()
    {
        foreach (Familiar f in _familiars)
            if (f != null) Destroy(f.gameObject);

        _familiars.Clear();
    }

    public bool IsShotgunActive => _isShotgunActive;
    public int FamiliarCount => _familiars.Count;

    private bool IsKeyPressed(string keyName)
    {
        if (string.IsNullOrEmpty(keyName) || Keyboard.current == null) return false;

        foreach (var key in Keyboard.current.allKeys)
        {
            if (key.name.Equals(keyName, System.StringComparison.OrdinalIgnoreCase))
            {
                return key.isPressed;
            }
        }
        return false;
    }
}