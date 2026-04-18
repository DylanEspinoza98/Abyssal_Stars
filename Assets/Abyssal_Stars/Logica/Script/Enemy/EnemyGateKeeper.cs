using UnityEngine;

public class EnemyGateKeeper : EnemyBase
{
    [Header("Ajustes de Movimiento")]
    [SerializeField] private float _horizontalSpeed = 3f;
    [SerializeField] private float _fallSpeed = 1.5f;
    [SerializeField] private float _horizontalLimit = 4.2f;

    [Header("Ataque")]
    [SerializeField] private Bullet _bulletPrefab;
    [SerializeField] private float _fireRate = 0.4f;
    [SerializeField] private float _bulletSpeed = 1.0f; // Ajusta esto a 0.5 o 1 para que sea lento

    private int _directionX = 1;
    private float _fireTimer;

    // Usamos 'protected override' para eliminar la advertencia CS0114
    protected override void Update()
    {

        base.Update();

        transform.Translate(Vector2.down * _fallSpeed * Time.deltaTime, Space.World);
        transform.Translate(Vector2.right * _directionX * _horizontalSpeed * Time.deltaTime, Space.World);

        if (transform.position.x >= _horizontalLimit) _directionX = -1;
        else if (transform.position.x <= -_horizontalLimit) _directionX = 1;

        HandleShooting();
    }

    private void HandleShooting()
    {
        _fireTimer += Time.deltaTime;
        if (_fireTimer >= _fireRate)
        {
            if (BulletPool.Instance != null)
            {

                Vector2 bulletVelocity = Vector2.down * _bulletSpeed;

                BulletPool.Instance.GetBullet(_bulletPrefab, transform.position, Quaternion.identity, bulletVelocity);
            }
            _fireTimer = 0;
        }
    }
}