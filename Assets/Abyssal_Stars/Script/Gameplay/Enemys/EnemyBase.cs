using UnityEngine;
using System.Collections;

public abstract class EnemyBase : MonoBehaviour
{
    protected EnemyPool _myPool;
    protected GameObject _myPrefabKey;

    [Header("Vida")]
    [SerializeField] private int _maxHealth = 0;
    private int _currentHealth;

    [Header("Reciclaje")]
    [SerializeField] private float _offScreenMargin = 0.3f;

    [Header("Feedback Visual")]
    [SerializeField] private Color _hitColor = Color.red;
    [SerializeField] private float _hitFlashDuration = 0.1f;

    [Header("Puntuación")]
    [SerializeField] protected int _scoreValue = 100;

    [Header("Feedback Visual")]
    [SerializeField] protected GameObject _explosionEffectPrefab;

    [Header("Huida del Jefe")]
    [SerializeField] private float _retreatSpeed = 15f;

    [Header("Configuración de Escudo (protección de enemigos)")]
    [Tooltip("Tamaño exacto del escudo para este enemigo. Ajustar a ojo en el Inspector.")]
    public float personalShieldSize = 1.5f;


    protected bool _isRetreating = false;
    private Vector3 _retreatDirection;

    private SpriteRenderer _sr;
    private Color _originalColor;
    private bool _isDefeated = false;

    public void Setup(EnemyPool pool, GameObject prefabKey)
    {
        _myPool = pool;
        _myPrefabKey = prefabKey;
    }

    protected virtual void OnEnable()
    {
        _isRetreating = false;
        _currentHealth = _maxHealth;
        _isDefeated = false;

        BossPhaseController.OnBossWarning += IniciarRetirada;

        if (_sr == null)
        {
            _sr = GetComponent<SpriteRenderer>();
            if (_sr != null) _originalColor = _sr.color;
        }

        if (_sr != null) _sr.color = _originalColor;
    }


    protected virtual void Update()
    {
        if (IsOutOfScreen())
        {
            ReturnToPool();
            return;
        }

        if (_isRetreating)
        {
            transform.Translate(_retreatDirection * _retreatSpeed * Time.deltaTime, Space.World);
            return;
        }
    }

    private bool IsOutOfScreen()
    {
        if (Camera.main == null) return false;
        Vector2 vp = Camera.main.WorldToViewportPoint(transform.position);

        return vp.x < -_offScreenMargin || vp.x > 1 + _offScreenMargin ||
               vp.y < -_offScreenMargin || vp.y > 1 + _offScreenMargin;
    }


    protected virtual void OnDisable()
    {
        BossPhaseController.OnBossWarning -= IniciarRetirada;
    }


    public void TakeDamage(int amount)
    {
        if (_isDefeated) return;

        if (_maxHealth <= 0)
        {
            Die();
            return;
        }

        _currentHealth -= amount;

        if (_currentHealth <= 0)
        {
            Die();
        }
        else
        {
            if (_sr != null) StartCoroutine(HitFlash());
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_isDefeated || _isRetreating) return;

        if (collision.CompareTag("Player"))
        {
            PlayerHealth player = collision.GetComponent<PlayerHealth>();
            if (player != null)
            {
                if (player.IsInvincible) return;
                player.TakeDamage(1);
            }

            if (!gameObject.CompareTag("Boss"))
            {
                Die();
            }
        }
    }

    protected virtual void Die()
    {
        if (_isDefeated) return;
        _isDefeated = true;

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(_scoreValue);
        }

        if (_explosionEffectPrefab != null)
            Instantiate(_explosionEffectPrefab, transform.position, Quaternion.identity);

        ReturnToPool();
    }

    private IEnumerator HitFlash()
    {
        _sr.color = _hitColor;
        yield return new WaitForSeconds(_hitFlashDuration);
        _sr.color = _originalColor;
    }

    private void IniciarRetirada()
    {
        if (gameObject.CompareTag("Enemy"))
        {
            _isRetreating = true;

            float dirX = (transform.position.x > 0) ? 1f : -1f;

            _retreatDirection = new Vector3(dirX, 0.5f, 0f).normalized;

            StopAllCoroutines();
        }
    }

    public virtual void ReturnToPool()
    {
        SpawnManager.NotifyDefeated(_myPrefabKey);

        if (_myPool != null)
            _myPool.ReturnEnemy(_myPrefabKey, this);
        else
            Destroy(gameObject);
    }
}