using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System; 

public class playerScript : MonoBehaviour

{
    public static playerScript Instance { get; private set; }

    [Header("Movement")]
    public float movSpeed = 5f;
    [SerializeField] private float _focusSpeedMultiplier = 0.4f; 
    [SerializeField] private float _limitX = 4.5f;
    [SerializeField] private float _limitY = 8.5f;

    [Header("Shooting Settings")]
    [SerializeField] private PlayerBullet _bulletPrefab;
    [SerializeField] private float _fireRate = 0.15f;
    [SerializeField] private float _bulletSpeed = 12f;
    private float _fireTimer;

    [Header("Arcade Life Settings")]
    [SerializeField] private int _totalLives = 3;
    public int TotalLives => _totalLives;
    [SerializeField] private GameObject _explosionEffectPrefab;
    [SerializeField] private float _respawnTime = 2f;
    [SerializeField] private float _invincibilityDuration = 3f;

    private Vector3 _startPosition;
    private bool _isDead = false;
    private bool _isInvincible = false;

    public bool IsInvincible => _isInvincible;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Collider2D col;
    

    public event Action<int> OnLivesChanged;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        _startPosition = new Vector3(0, 0, transform.localPosition.z);
    }

    void Update()
    {
        if (_isDead) return;
        HandleMovement();
        HandleShooting();
    }

    private void HandleMovement()
    {
        Vector2 moveInput = Vector2.zero;
        float currentSpeed = movSpeed;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.leftShiftKey.isPressed)
            {
                currentSpeed *= _focusSpeedMultiplier;
            }

            if (Keyboard.current.wKey.isPressed) moveInput.y = 1;
            if (Keyboard.current.sKey.isPressed) moveInput.y = -1;
            if (Keyboard.current.aKey.isPressed) moveInput.x = -1;
            if (Keyboard.current.dKey.isPressed) moveInput.x = 1;
        }

        rb.linearVelocity = moveInput.normalized * currentSpeed;

        float clampedX = Mathf.Clamp(transform.localPosition.x, -_limitX, _limitX);
        float clampedY = Mathf.Clamp(transform.localPosition.y, -_limitY, _limitY);

        transform.localPosition = new Vector3(clampedX, clampedY, transform.localPosition.z);
    }

    public void TakeDamage(int amount)
    {
        if (_isDead || _isInvincible) return;
        _totalLives--;
        OnLivesChanged?.Invoke(_totalLives);
        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        _isDead = true;
        if (_explosionEffectPrefab != null)
            Instantiate(_explosionEffectPrefab, transform.position, Quaternion.identity);

        sr.enabled = false;
        col.enabled = false;
        rb.linearVelocity = Vector2.zero;

        if (_totalLives > 0)
        {
            yield return new WaitForSeconds(_respawnTime);
            Respawn();
        }
        else
        {
            if (GameOverManager.Instance != null)
                GameOverManager.Instance.ShowGameOver();
        }
    }

    private void Respawn()
    {
        transform.localPosition = _startPosition;
        sr.enabled = true;
        col.enabled = true;
        _isDead = false;
        StartCoroutine(InvincibilityRoutine());
    }

    private IEnumerator InvincibilityRoutine()
    {
        _isInvincible = true;
        float timer = 0;
        while (timer < _invincibilityDuration)
        {
            sr.enabled = !sr.enabled;
            yield return new WaitForSeconds(0.1f);
            timer += 0.1f;
        }
        sr.enabled = true;
        _isInvincible = false;
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