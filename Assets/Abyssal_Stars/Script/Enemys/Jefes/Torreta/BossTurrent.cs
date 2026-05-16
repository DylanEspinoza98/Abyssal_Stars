using UnityEngine;
using System.Collections;

public class BossTurret : MonoBehaviour
{
    [Header("Munición")]
    public EnemyBullet bulletPrefab;
    public Sprite bulletSprite;
    public Color bulletColor = Color.white;

    [Header("Lista de Reproducción de Patrones")]

    [SerializeField] private AttackPatternSO[] _patternPlaylist;
    [SerializeField] private float _timePerPattern = 5f;
    [SerializeField] private float _transitionDelay = 2f;

    private void Start()
    {
        if (_patternPlaylist.Length > 0)
        {
            StartCoroutine(CyclePatternsRoutine());
        }
    }

    private IEnumerator CyclePatternsRoutine()
    {
        int currentIndex = 0;

        while (true)
        {
            AttackPatternSO currentPattern = _patternPlaylist[currentIndex];

            Coroutine activePattern = StartCoroutine(currentPattern.ExecutePattern(this));

            yield return new WaitForSeconds(_timePerPattern);

            StopCoroutine(activePattern);

            yield return new WaitForSeconds(_transitionDelay);

            currentIndex = (currentIndex + 1) % _patternPlaylist.Length;
        }
    }

    public void FireSingleBullet(float exactAngle, float speed)
    {
        if (bulletPrefab == null) return;

        float dirX = Mathf.Cos(exactAngle * Mathf.Deg2Rad);
        float dirY = Mathf.Sin(exactAngle * Mathf.Deg2Rad);
        Vector2 bulletDir = new Vector2(dirX, dirY).normalized;

        EnemyBullet bullet = BulletPool.Instance.GetBullet(bulletPrefab, transform.position, Quaternion.identity, bulletDir * speed);

        if (bullet != null)
        {
            bullet.SetRotationByVelocity();
            if (bulletSprite != null) bullet.SetAppearance(bulletSprite, bulletColor);
        }
    }
}