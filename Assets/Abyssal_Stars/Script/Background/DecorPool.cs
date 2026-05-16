using UnityEngine;
using System.Collections.Generic;

public class DecorPool
{
    private readonly GameObject    _prefab;
    private readonly Transform     _poolParent;
    private readonly Stack<GameObject> _available = new Stack<GameObject>();

    public int TotalCreated  { get; private set; }
    public int ActiveCount   { get; private set; }
    public int AvailableCount => _available.Count;

    public DecorPool(GameObject prefab, Transform poolParent, int initialSize = 5)
    {
        _prefab     = prefab;
        _poolParent = poolParent;

        for (int i = 0; i < initialSize; i++)
            CreateAndStore();
    }
    public GameObject Get(Vector3 position, Quaternion rotation)
    {
        GameObject obj = _available.Count > 0
            ? _available.Pop()
            : CreateNew();

        obj.transform.SetPositionAndRotation(position, rotation);
        obj.SetActive(true);
        ActiveCount++;
        return obj;
    }

    public void Return(GameObject obj)
    {
        if (obj == null) return;

        obj.SetActive(false);
        obj.transform.SetParent(_poolParent, worldPositionStays: false);
        _available.Push(obj);
        ActiveCount = Mathf.Max(0, ActiveCount - 1);
    }

    private GameObject CreateNew()
    {
        GameObject obj = Object.Instantiate(_prefab, _poolParent);
        obj.SetActive(false);
        TotalCreated++;
        return obj;
    }

    private void CreateAndStore()
    {
        _available.Push(CreateNew());
    }
}
