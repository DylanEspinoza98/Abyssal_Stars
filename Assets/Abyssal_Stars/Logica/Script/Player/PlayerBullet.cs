using UnityEngine;

public class PlayerBullet : Bullet
{
    [SerializeField] private int _damage = 1;

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            // Primero intenta golpear enemigos normales (Kamikaze, Circular, etc.)
            EnemyBase enemy = collision.GetComponent<EnemyBase>();
            if (enemy != null)
            {
                enemy.TakeDamage(_damage);
                base.OnTriggerEnter2D(collision);
                return;
            }

            // Si no es EnemyBase, intenta con BossController (para cuando lo tengas)
            BossController boss = collision.GetComponent<BossController>();
            if (boss != null)
            {
                boss.TakeDamage(_damage);
                base.OnTriggerEnter2D(collision);
                return;
            }
        }

        base.OnTriggerEnter2D(collision);
    }
}
