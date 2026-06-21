using UnityEngine;

public class EnemyBullet : Bullet
{
    [Header("Configuraci�n de Da�o")]
    [SerializeField] private int _damage = 1;

    [Header("Capacidades Defensivas")]
    [Tooltip("Si est� activo, esta bala actuar� como escudo y destruir� los disparos del jugador al chocar.")]
    [SerializeField] private bool _canDestroyPlayerBullets = false;

    public bool CanDestroyPlayerBullets => _canDestroyPlayerBullets;
    public void SetShieldMode(bool active)
    {
        _canDestroyPlayerBullets = active;
        gameObject.tag = active ? "BulletShield" : "Bullet";
    }

    private SpriteRenderer _sr;
    private Rigidbody2D _rb;
    public bool HasBeenGrazed { get; set; } = false;

    protected override void Awake()
    {
        base.Awake();
        _sr = GetComponent<SpriteRenderer>();
        _rb = GetComponent<Rigidbody2D>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        HasBeenGrazed = false;
        gameObject.tag = "Bullet";
        gameObject.layer = LayerMask.NameToLayer("Disparos Amenazas");
        BossPhaseController.OnBossWarning += OnBossWarning;
    }

    private void OnDisable()
    {
        BossPhaseController.OnBossWarning -= OnBossWarning;
    }

    private void OnBossWarning()
    {
        ReturnToPool();
    }

    public void SetAppearance(Sprite newSprite, Color newColor)
    {
        if (_sr != null)
        {
            _sr.sprite = newSprite;
            _sr.color = newColor;
        }
    }

    public void Fire(Vector2 direction, float speed)
    {
        Vector2 vel = direction.normalized * speed;

        Velocity = vel;

        if (_rb != null)
        {
            _rb.bodyType = RigidbodyType2D.Kinematic;
            _rb.linearVelocity = Vector2.zero;
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Graze"))
        {
            return;
        }

        if (collision.CompareTag("Player"))
        {
            PlayerHealth player = collision.GetComponent<PlayerHealth>();
            if (player != null) player.TakeDamage(_damage);
            base.OnTriggerEnter2D(collision);
            return;
        }

        if (collision.CompareTag("PlayerBullet"))
            return;

        base.OnTriggerEnter2D(collision);
    }

    public override void ResetBullet()
    {
        base.ResetBullet();
        SetShieldMode(false);
    }
}