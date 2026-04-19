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

    private SpriteRenderer _sr;
    private Color _originalColor;

    public void Setup(EnemyPool pool, GameObject prefabKey)
    {
        _myPool = pool;
        _myPrefabKey = prefabKey;
    }

    protected virtual void OnEnable()
    {
        _currentHealth = _maxHealth;

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
        }
    }

    private bool IsOutOfScreen()
    {
        if (Camera.main == null) return false;
        Vector2 vp = Camera.main.WorldToViewportPoint(transform.position);

        return vp.x < -_offScreenMargin || vp.x > 1 + _offScreenMargin ||
               vp.y < -_offScreenMargin || vp.y > 1 + _offScreenMargin;
    }


    public void TakeDamage(int amount)
    {
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
        if (collision.CompareTag("Player"))
        {
            playerScript player = collision.GetComponent<playerScript>();

            if (player != null)
            {
                if (player.IsInvincible) return;

                player.TakeDamage(1);
            }

            if (!gameObject.CompareTag("Boss"))
            {
                ReturnToPool();
            }
        }
    }

    protected virtual void Die()
    {
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

    public virtual void ReturnToPool()
    {
        if (_myPool != null)
            _myPool.ReturnEnemy(_myPrefabKey, this);
        else
            Destroy(gameObject);
    }
}