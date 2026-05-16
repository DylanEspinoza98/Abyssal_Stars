using UnityEngine;
using System.Collections;
using System;

public class PlayerHealth : MonoBehaviour
{
    public static PlayerHealth Instance { get; private set; }

    [Header("Arcade Life Settings")]
    [SerializeField] private int _totalLives = 3;
    [SerializeField] private int _maxLives = 5;  // tope maximo de vidas
    public int TotalLives => _totalLives;

    [SerializeField] private GameObject _explosionEffectPrefab;
    [SerializeField] private float _respawnTime = 2f;
    [SerializeField] private float _invincibilityDuration = 3f;

    private Vector3 _startPosition;
    private bool _isDead = false;
    private bool _isInvincible = false;

    public bool IsDead => _isDead;
    public bool IsInvincible => _isInvincible;

    private SpriteRenderer[] _renderers;
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
        _renderers = GetComponentsInChildren<SpriteRenderer>();
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

    public void AddLife()
    {
        if (_totalLives >= _maxLives) return;
        _totalLives++;
        OnLivesChanged?.Invoke(_totalLives);
    }

    private IEnumerator DeathSequence()
    {
        _isDead = true;

        if (_explosionEffectPrefab != null)
            Instantiate(_explosionEffectPrefab, transform.position, Quaternion.identity);

        foreach (SpriteRenderer sr in _renderers)
            if (sr != null) sr.enabled = false;

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

        foreach (SpriteRenderer sr in _renderers)
            if (sr != null) sr.enabled = true;

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
            foreach (SpriteRenderer sr in _renderers)
                if (sr != null) sr.enabled = !sr.enabled;

            yield return new WaitForSeconds(0.1f);
            timer += 0.1f;
        }

        foreach (SpriteRenderer sr in _renderers)
            if (sr != null) sr.enabled = true;

        _isInvincible = false;
    }
}