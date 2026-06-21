using UnityEngine;

public class PlayerBullet : Bullet
{
    [SerializeField] private int _damage = 1;
    private int _baseDamage;

    protected override void Awake()
    {
        base.Awake();
        _baseDamage = _damage;
    }
    public override void ResetBullet()
    {
        base.ResetBullet();
        _damage = _baseDamage;
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy") || collision.CompareTag("Boss"))
        {
            EnemyBase enemy = collision.GetComponent<EnemyBase>();

            if (enemy != null)
            {
                enemy.TakeDamage(_damage);
                base.OnTriggerEnter2D(collision);
                return;
            }
        }

        if (collision.CompareTag("BulletShield"))
        {
            ReturnToPool();
            return;
        }

        if (collision.CompareTag("Bullet"))
            return;

        base.OnTriggerEnter2D(collision);
    }
}