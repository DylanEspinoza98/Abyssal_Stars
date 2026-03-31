using System.Collections.Generic;
using UnityEngine;

public class BulletPool : MonoBehaviour
{
    public static BulletPool Instance { get; private set; }

    private Dictionary<GameObject, Queue<Bullet>> _pools = new Dictionary<GameObject, Queue<Bullet>>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public T GetBullet<T>(T prefab, Vector3 position, Quaternion rotation, Vector2 velocity) where T : Bullet
    {
        GameObject prefabKey = prefab.gameObject;

        if (!_pools.ContainsKey(prefabKey))
        {
            _pools.Add(prefabKey, new Queue<Bullet>());
        }

        Bullet bullet;

        if (_pools[prefabKey].Count > 0)
        {
            bullet = _pools[prefabKey].Dequeue();
        }
        else
        {
            bullet = Instantiate(prefab);
            bullet.Setup(this, prefabKey);
            // IMPORTANTE: Le pasamos los datos del Pool para que sepa a dónde volver
        }

        bullet.transform.position = position;
        bullet.transform.rotation = rotation;
        bullet.Velocity = velocity;        // Asignamos velocidad primero
        bullet.gameObject.SetActive(true); // Luego activamos (dispara el OnEnable)

        return (T)bullet;
    }

    public void ReturnBullet(GameObject prefabKey, Bullet bullet)
    {
        if (_pools.ContainsKey(prefabKey))
        {
            _pools[prefabKey].Enqueue(bullet);
        }
        bullet.gameObject.SetActive(false);
    }
}