using UnityEngine;
using System.Collections;

public abstract class EnemyBase : MonoBehaviour
{
    protected EnemyPool _myPool;
    protected GameObject _myPrefabKey;

    [Header("Vida (0 = sin vida, muere al primer golpe)")]
    [SerializeField] private int _maxHealth = 0;
    private int _currentHealth;

    [Header("Feedback Visual")]
    [SerializeField] private GameObject _explosionEffectPrefab;
    [SerializeField] private Color _hitColor = Color.red;
    [SerializeField] private float _hitFlashDuration = 0.1f;

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

    public void TakeDamage(int amount)
    {
        // Sin vida configurada = muere al instante
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
            // Flash de daño
            if (_sr != null)
                StartCoroutine(HitFlash());
        }
    }

  
    private void Die()
    {
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
