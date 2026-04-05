using UnityEngine;

public abstract class Bullet : MonoBehaviour
{
    [Header("Base Settings")]
    [SerializeField] protected float _maxLifeTime = 10f;
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
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, 0f);
    }

    protected virtual void Update()
    {
        transform.position += (Vector3)Velocity * Time.deltaTime;
        _currentLifeTime += Time.deltaTime;

        if (_currentLifeTime >= _maxLifeTime)
        {
            ReturnToPool(); 
        }
    }

    
    protected void ReturnToPool()
    {
        if (_myPool != null)
        {
            _myPool.ReturnBullet(_myPrefabKey, this);
        }
        else
        {
            gameObject.SetActive(false); 
        }
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        ReturnToPool(); 
    }
}