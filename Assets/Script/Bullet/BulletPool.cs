using System;
using System.Collections.Generic;
using UnityEngine;


public class BulletPool : MonoBehaviour
{
    private static BulletPool _instance;

    public static BulletPool Instance
    {
        get
        {
            if (_instance == null)
               Debug.LogError("BulletPool Es Null, ERROR.");

            return _instance;
        }

        
    }

    [SerializeField] private Bullet _bulletPrefab;
    [SerializeField] private int _initialPoolSize =10;

    private List<Bullet> _bulletPool = new List<Bullet>();

    private void Awake()
    {
        //RECORDAR: SETUP DEL SINGLETON

        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            _instance = this;
        }
        //RECORDAR:SETUP DEL POOL
        AddBulletsToPool(_initialPoolSize);
    }

    private void AddBulletsToPool(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            Bullet Bullet = Instantiate(_bulletPrefab);
            Bullet.gameObject.SetActive(false);
            _bulletPool.Add(Bullet);
            Bullet.transform.parent = transform;

        }
    }

    public Bullet RequestBullet()
    {
        for (int i = 0; i < _bulletPool.Count; i++)
        {
            if (!_bulletPool[i].gameObject.activeSelf)
            {
                _bulletPool[i].gameObject.SetActive(true);
                return _bulletPool[i];
            }
        }
        AddBulletsToPool(1);
        _bulletPool[_bulletPool.Count - 1].gameObject.SetActive(true);
        return _bulletPool[_bulletPool.Count - 1];
    }

}
