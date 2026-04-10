using UnityEngine;

public class EnemyBullet : Bullet
{
    [SerializeField] private int _damage = 1;

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        // 1. Buscamos el Tag "Player"
        if (collision.CompareTag("Player"))
        {
            // 2. Ahora buscamos el script unificado "playerScript"
            playerScript player = collision.GetComponent<playerScript>();

            if (player != null)
            {
                player.TakeDamage(_damage);
            }
        }

        // 3. Importante: Ejecutar la lógica base para volver al Pool
        base.OnTriggerEnter2D(collision);
    }
}