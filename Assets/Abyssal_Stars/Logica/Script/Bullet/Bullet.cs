using UnityEngine;

public abstract class Bullet : MonoBehaviour
{
    [Header("Base Settings")]
    [SerializeField] protected float _maxLifeTime = 10f;

    // Margen extra fuera de pantalla antes de reciclar (evita pop-in brusco)
    [SerializeField] private float _offScreenMargin = 0.1f;

    private float _currentLifeTime;
    private BulletPool _myPool;
    private GameObject _myPrefabKey;

    public Vector2 Velocity { get; set; }

    public void Setup(BulletPool pool, GameObject prefabKey)
    {
        _myPool = pool;
        _myPrefabKey = prefabKey;
    }

    protected virtual void OnEnable()
    {
        _currentLifeTime = 0f;
        transform.localPosition = new Vector3(
            transform.localPosition.x,
            transform.localPosition.y,
            0f
        );
    }

    protected virtual void Update()
    {
        transform.position += (Vector3)Velocity * Time.deltaTime;
        _currentLifeTime += Time.deltaTime;

        // Reciclar si sale de pantalla o excede el tiempo de vida
        if (IsOutOfScreen() || _currentLifeTime >= _maxLifeTime)
        {
            ReturnToPool();
        }
    }

    /// <summary>
    /// Devuelve true si la bala salio de los limites de la camara.
    /// Viewport: (0,0) = esquina inferior izquierda, (1,1) = superior derecha.
    /// </summary>
    private bool IsOutOfScreen()
    {
        if (Camera.main == null) return false;

        Vector3 vp = Camera.main.WorldToViewportPoint(transform.position);
        return vp.x < -_offScreenMargin || vp.x > 1f + _offScreenMargin ||
               vp.y < -_offScreenMargin || vp.y > 1f + _offScreenMargin;
    }

    protected void ReturnToPool()
    {
        if (_myPool != null)
            _myPool.ReturnBullet(_myPrefabKey, this);
        else
            gameObject.SetActive(false);
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        ReturnToPool();
    }
}