using UnityEngine;

public class EnemyDummy : MonoBehaviour
{
    [SerializeField] private float _health = 3f;

    
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
        gameObject.SetActive(false);
    }
}