using UnityEngine;

public class EnemyDummy : MonoBehaviour
{
    [SerializeField] private float _health = 3f;

    // Este método se llamará desde la bala o por colisión propia
    public void TakeDamage(float amount)
    {
        _health -= amount;
        Debug.Log($"Enemigo golpeado! Vida restante: {_health}");

        if (_health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Enemigo Destruido");
        // Por ahora solo lo desactivamos, luego podrías usar un Pool para enemigos también
        gameObject.SetActive(false);
    }
}