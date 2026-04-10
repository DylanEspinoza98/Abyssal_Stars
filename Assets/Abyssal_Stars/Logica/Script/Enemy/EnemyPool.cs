using System.Collections.Generic;
using UnityEngine;

public class EnemyPool : MonoBehaviour
{
    public static EnemyPool Instance { get; private set; }

    private Dictionary<GameObject, Queue<EnemyBase>> _pools = new Dictionary<GameObject, Queue<EnemyBase>>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public T GetEnemy<T>(T prefab, Vector3 position, Quaternion rotation) where T : EnemyBase
    {
        GameObject prefabKey = prefab.gameObject;

        if (!_pools.ContainsKey(prefabKey))
            _pools.Add(prefabKey, new Queue<EnemyBase>());

        EnemyBase enemy;
        if (_pools[prefabKey].Count > 0)
            enemy = _pools[prefabKey].Dequeue();
        else
        {
            enemy = Instantiate(prefab);
            enemy.Setup(this, prefabKey);
        }

        //  EnemyPool como padre, no Camera.main
        // Cambia la línea en tu GetEnemy:
        enemy.transform.SetParent(Camera.main.transform);
        enemy.transform.position = position;
        enemy.transform.rotation = rotation;
        enemy.gameObject.SetActive(true);

        return (T)enemy;
    }

    public void ReturnEnemy(GameObject prefabKey, EnemyBase enemy)
    {
        if (_pools.ContainsKey(prefabKey))
            _pools[prefabKey].Enqueue(enemy);

        enemy.gameObject.SetActive(false);
    }
}