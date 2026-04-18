using UnityEngine;

public class PlayerBullet : Bullet
{
    [SerializeField] private int _damage = 1;

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

        base.OnTriggerEnter2D(collision);
    }
}
