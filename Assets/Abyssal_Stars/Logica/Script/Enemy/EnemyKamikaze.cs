using UnityEngine;

public class Kamikaze : EnemyBase
{
    [Header("Movement")]
    [SerializeField] private float _speed = 5f;
    [SerializeField] private float _lowerLimitOffset = 12f; // Unidades bajo la cámara

    protected override void Update()
    {
       
        base.Update();
        transform.Translate(Vector3.down * _speed * Time.deltaTime, Space.World);

        float cameraBottom = Camera.main.transform.position.y - _lowerLimitOffset;
        if (transform.position.y < cameraBottom)
            ReturnToPool();
    }

}