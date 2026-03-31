using UnityEngine;

public abstract class Bullet : MonoBehaviour
{
    [Header("Base Settings")]
    [SerializeField] protected float _maxLifeTime = 10f;
    private float _currentLifeTime;

    // --- NUEVAS VARIABLES PARA EL POOLING ---
    private BulletPool _myPool;
    private GameObject _myPrefabKey;
    // ----------------------------------------

    public Vector2 Velocity { get; set; }

    // El Pool llamará a esto UNA SOLA VEZ al crear la bala
    public void Setup(BulletPool pool, GameObject prefabKey)
    {
        _myPool = pool;
        _myPrefabKey = prefabKey;
    }

    protected virtual void OnEnable()
    {
        _currentLifeTime = 0f;
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, 0f);
    }

    protected virtual void Update()
    {
        transform.position += (Vector3)Velocity * Time.deltaTime;
        _currentLifeTime += Time.deltaTime;

        if (_currentLifeTime >= _maxLifeTime)
        {
            ReturnToPool(); // <-- CAMBIO AQUÍ
        }
    }

    // Método para devolver la bala al "almacén" correctamente
    protected void ReturnToPool()
    {
        if (_myPool != null)
        {
            _myPool.ReturnBullet(_myPrefabKey, this);
        }
        else
        {
            gameObject.SetActive(false); // Por si acaso no hay pool
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        ReturnToPool(); 
    }
}