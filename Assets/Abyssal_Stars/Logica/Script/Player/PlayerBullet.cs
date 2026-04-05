using UnityEngine;


public class PlayerBullet : Bullet
{
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            BossController enemy = collision.GetComponent<BossController>();
            if (enemy != null)
            {
                enemy.TakeDamage(1f);
            }
        }

        base.OnTriggerEnter2D(collision);
    }
}