using UnityEngine;

public class PlayerGraze : MonoBehaviour
{
    [Header("Recompensas")]
    [Tooltip("Puntos otorgados por rozar una bala.")]
    [SerializeField] private int _grazeScore = 50;

    [Header("Feedback Sonoro")]
    [SerializeField] private AudioClip _grazeSound;
    [SerializeField] private float _soundVolume = 0.6f;

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (PlayerHealth.Instance != null &&
           (PlayerHealth.Instance.IsDead || PlayerHealth.Instance.IsInvincible))
            return;

        if (collision.CompareTag("Bullet") || collision.CompareTag("BulletShield"))
        {
            EnemyBullet enemyBullet = collision.GetComponent<EnemyBullet>();

            if (enemyBullet != null && !enemyBullet.HasBeenGrazed)
            {
                enemyBullet.HasBeenGrazed = true;
                ApplyGraze();
            }
        }
    }

    private void ApplyGraze()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(_grazeScore);
        }
        if (_grazeSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(_grazeSound, _soundVolume);
        }
    }
}