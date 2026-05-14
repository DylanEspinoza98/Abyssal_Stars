using UnityEngine;

public class EnemyBullet : Bullet
{
    [SerializeField] private int _damage = 1;
    private SpriteRenderer _sr;

    public void SetAppearance(Sprite newSprite, Color newColor)
    {
        if (_sr == null) _sr = GetComponent<SpriteRenderer>();

        if (_sr != null)
        {
            _sr.sprite = newSprite;
            _sr.color = newColor;
        }
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerHealth player = collision.GetComponent<PlayerHealth>();
            if (player != null) player.TakeDamage(_damage);
        }

        base.OnTriggerEnter2D(collision);
    }
}