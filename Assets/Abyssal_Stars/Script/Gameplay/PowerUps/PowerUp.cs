using UnityEngine;

// Clase base para todos los power ups
public abstract class PowerUp : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float _fallSpeed = 2.5f;
    [SerializeField] private float _offScreenMargin = 0.3f;
    [Header("Detección")]
    [SerializeField] private float _pickupRadius = 0.5f;

    private bool _collected = false;

    void Update()
    {
        // Cae hacia abajo
        transform.Translate(Vector2.down * _fallSpeed * Time.deltaTime);

        
        if (Camera.main != null)
        {
            Vector2 vp = Camera.main.WorldToViewportPoint(transform.position);
            if (vp.y < -_offScreenMargin)
            {
                Destroy(gameObject);
                return;
            }
        }

        
        if (!_collected && PlayerHealth.Instance != null && !PlayerHealth.Instance.IsDead)
        {
            float dist = Vector2.Distance(transform.position, PlayerHealth.Instance.transform.position);
            if (dist <= _pickupRadius)
            {
                _collected = true;
                PlayerShooter shooter = PlayerHealth.Instance.GetComponent<PlayerShooter>();
                OnCollected(PlayerHealth.Instance, shooter);
                Destroy(gameObject);
            }
        }
    }

    // Cada power up implementa su efecto específico al ser recogido
    protected abstract void OnCollected(PlayerHealth player, PlayerShooter shooter);
}