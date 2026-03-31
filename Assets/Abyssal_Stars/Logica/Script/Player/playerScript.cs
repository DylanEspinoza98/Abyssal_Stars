using UnityEngine;
using UnityEngine.InputSystem;

public class playerScript : MonoBehaviour
{
    [Header("Movement")]
    public float movSpeed = 5f;
    private Rigidbody2D rb;

    [Header("Shooting Settings")]
    [SerializeField] private PlayerBullet _bulletPrefab;
    [SerializeField] private float _fireRate = 0.15f;
    [SerializeField] private float _bulletSpeed = 12f;

    private float _fireTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        HandleMovement();
        HandleShooting();
    }

    private void HandleMovement()
    {
        Vector2 moveInput = Vector2.zero;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed) moveInput.y = 1;
            if (Keyboard.current.sKey.isPressed) moveInput.y = -1;
            if (Keyboard.current.aKey.isPressed) moveInput.x = -1;
            if (Keyboard.current.dKey.isPressed) moveInput.x = 1;
        }

        rb.linearVelocity = moveInput.normalized * movSpeed;
    }

    private void HandleShooting()
    {
        if (_fireTimer > 0)
            _fireTimer -= Time.deltaTime;

        if (Keyboard.current != null && Keyboard.current.spaceKey.isPressed && _fireTimer <= 0)
        {
            Shoot();
            _fireTimer = _fireRate;
        }
    }

    private void Shoot()
    {
        Vector2 velocity = transform.up * _bulletSpeed;
        BulletPool.Instance.GetBullet(_bulletPrefab, transform.position, transform.rotation, velocity);
    }
}