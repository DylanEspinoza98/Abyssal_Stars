using UnityEngine;

public class EnemyBullet : Bullet
{
    [SerializeField] private int _damage = 1;
    private SpriteRenderer _sr;
    private bool _isHarmless = false;

    protected override void OnEnable()
    {
        base.OnEnable();
        _isHarmless = false;

        SpawnManager.OnBossFightStarted += VolverInofensiva;
    }

    private void OnDisable()
    {
        SpawnManager.OnBossFightStarted -= VolverInofensiva;
    }

    public void SetAppearance(Sprite newSprite, Color newColor)
    {
        if (_sr == null) _sr = GetComponent<SpriteRenderer>();

        if (_sr != null)
        {
            _sr.sprite = newSprite;
            _sr.color = newColor;
        }
    }

    private void VolverInofensiva()
    {
        _isHarmless = true;

        if (_sr == null) _sr = GetComponent<SpriteRenderer>();
        if (_sr != null)
        {
            Color c = _sr.color;
            c.a = 0.4f;
            _sr.color = c;
        }
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (_isHarmless && collision.CompareTag("Player"))
        {
            return;
        }

        if (collision.CompareTag("Player"))
        {
            PlayerHealth player = collision.GetComponent<PlayerHealth>();
            if (player != null) player.TakeDamage(_damage);
        }

        base.OnTriggerEnter2D(collision);
    }
}