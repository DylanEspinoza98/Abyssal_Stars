using UnityEngine;

public abstract class Bullet : MonoBehaviour
{
    [Header("Base Settings")]
    [SerializeField] protected float _maxLifeTime = 10f;
    [SerializeField] private float _offScreenMargin = 0.1f;

    [Header("Bullet Hell Settings")]
    [Tooltip("Segundos antes de que la bala empiece a auto-destruirse por salir de la cámara. " +
             "Solo aplica si la bala YA entró a pantalla al menos una vez.")]
    [SerializeField] private float _graceTime = 1.5f;

    private float _currentLifeTime;
    private BulletPool _myPool;
    private GameObject _myPrefabKey;

    private bool _isEntering = true;

    private bool _hasKillPoint = false;
    private Vector2 _killPoint;
    private float _killRadiusSqr;

    protected SpriteRenderer _spriteRenderer;
    protected Color _originalColor;
    public Vector2 Velocity { get; set; }

    protected virtual void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_spriteRenderer != null)
        {
            _originalColor = _spriteRenderer.color;
        }
    }

    public void Setup(BulletPool pool, GameObject prefabKey)
    {
        _myPool = pool;
        _myPrefabKey = prefabKey;
    }
    public void SetKillPoint(Vector2 point, float radius)
    {
        _hasKillPoint = true;
        _killPoint = point;
        _killRadiusSqr = radius * radius;
    }

    protected virtual void OnEnable()
    {
        _currentLifeTime = 0f;
        _isEntering = true;
        _hasKillPoint = false; 

        transform.localPosition = new Vector3(
            transform.localPosition.x,
            transform.localPosition.y,
            0f
        );
    }

    public void SetRotationByVelocity()
    {
        if (Velocity != Vector2.zero)
        {
            float angle = Mathf.Atan2(Velocity.y, Velocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
        }
    }

    protected virtual void Update()
    {
        transform.position += (Vector3)Velocity * Time.deltaTime;
        _currentLifeTime += Time.deltaTime;

        if (_hasKillPoint)
        {
            Vector2 diff = (Vector2)transform.position - _killPoint;
            if (diff.sqrMagnitude <= _killRadiusSqr)
            {
                ReturnToPool();
                return;
            }
        }

        bool onScreen = IsOnScreen();

        if (_isEntering)
        {
            if (onScreen) _isEntering = false;

            if (_currentLifeTime >= _maxLifeTime)
                ReturnToPool();

            return;
        }

        if ((!onScreen && _currentLifeTime > _graceTime) || _currentLifeTime >= _maxLifeTime)
            ReturnToPool();
    }

    private bool IsOnScreen()
    {
        if (Camera.main == null) return false;

        Vector3 vp = Camera.main.WorldToViewportPoint(transform.position);
        return vp.x >= -_offScreenMargin && vp.x <= 1f + _offScreenMargin &&
               vp.y >= -_offScreenMargin && vp.y <= 1f + _offScreenMargin;
    }

    public void ReturnToPool()
    {
        ResetBullet();

        if (_myPool != null)
            _myPool.ReturnBullet(_myPrefabKey, this);
        else
            gameObject.SetActive(false);
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        ReturnToPool();
    }

    public virtual void ResetBullet()
    {
        if (_spriteRenderer != null)
        {
            _spriteRenderer.color = _originalColor;
        }

        transform.SetParent(null);

        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }
}