using UnityEngine;

// Hereda de Bullet, no de MonoBehaviour
public class PlayerBullet : Bullet
{
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. Lógica específica: ¿Es un enemigo?
        if (collision.CompareTag("Enemy"))
        {
            EnemyDummy enemy = collision.GetComponent<EnemyDummy>();
            if (enemy != null)
            {
                enemy.TakeDamage(1f);
            }
        }

        // 2. Ejecutar la lógica del padre (que es ReturnToPool)
        base.OnTriggerEnter2D(collision);
    }
}