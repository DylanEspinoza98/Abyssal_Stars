using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int _maxHealth = 3;
    private int _currentHealth;

    void Start()
    {
        _currentHealth = _maxHealth;
    }

    public void TakeDamage(int amount)
    {
        _currentHealth -= amount;
        Debug.Log($"Player golpeado! Vida restante: {_currentHealth}");

        if (_currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        Debug.Log("Player muerto!");
        gameObject.SetActive(false);
    }
}