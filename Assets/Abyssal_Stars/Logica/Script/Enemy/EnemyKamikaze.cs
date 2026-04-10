using UnityEngine;

public class Kamikaze : EnemyBase
{
    [Header("Movement")]
    [SerializeField] private float _speed = 5f;
    [SerializeField] private float _lowerLimitOffset = 12f; // Unidades bajo la cámara

    void Update()
    {
        transform.Translate(Vector3.down * _speed * Time.deltaTime, Space.World);

        // Límite relativo a la cámara para que funcione aunque la cámara suba
        float cameraBottom = Camera.main.transform.position.y - _lowerLimitOffset;
        if (transform.position.y < cameraBottom)
            ReturnToPool();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerScript player = collision.GetComponent<playerScript>();
            if (player != null)
                player.TakeDamage(1);

            ReturnToPool();
        }
    }
}