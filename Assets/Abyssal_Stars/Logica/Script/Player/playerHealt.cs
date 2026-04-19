using UnityEngine;
using System.Collections;
using System;

public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth Instance { get; private set; }

    [Header("Arcade Life Settings")]
    [SerializeField] private int _totalLives = 3;
    public int TotalLives => _totalLives;
    [SerializeField] private GameObject _explosionEffectPrefab;
    [SerializeField] private float _respawnTime = 2f;
    [SerializeField] private float _invincibilityDuration = 3f;

    private Vector3 _startPosition;
    private bool _isDead = false;
    private bool _isInvincible = false;

    public bool IsDead => _isDead;
    public bool IsInvincible => _isInvincible;

    private SpriteRenderer sr;
    private Collider2D col;
    private Rigidbody2D rb;

    public event Action<int> OnLivesChanged;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        _startPosition = new Vector3(0, 0, transform.localPosition.z);
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
}