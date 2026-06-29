using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "New Ultimate Symphony", menuName = "Boss Patterns/Attack/Ultimate Symphony")]
public class PatternUltimateSymphonySO : AttackPatternSO
{
    [Header("Bajos (Low) - Anillos Expansivos")]
    public EnemyBullet lowBulletPrefab; 
    public int ringBulletCount = 20;
    public float ringSpeed = 4f;

    [Header("Medios (Mid) - Figuras Geométricas")]
    public EnemyBullet midBulletPrefab;
    [Tooltip("Velocidad de la punta de la estrella/polígono.")]
    public float midBulletSpeed = 8f;

    [Header("Altos (High) - Lluvia")]
    public EnemyBullet highBulletPrefab;
    [Tooltip("Desde qué altura Y cae la lluvia.")]
    public float highSpawnY = 8f;
    [Tooltip("Límites X de la pantalla para la lluvia (Ej. 3 para ir de -3 a 3).")]
    public float highSpawnXRange = 2.8f;
    public float highBulletSpeed = 3f;

    [Header("Sub-Bajos (SubLow)")]
    public EnemyBullet subLowBulletPrefab;
    public int subLowBulletCount = 36;
    public float subLowSpeed = 2f;

    private BossTurret _activeTurret;

    public override IEnumerator ExecutePattern(BossTurret turret)
    {
        _activeTurret = turret;

        if (AudioBeatDetector.Instance != null)
        {
            AudioBeatDetector.Instance.OnLowBeat += HandleLowBeat;
            AudioBeatDetector.Instance.OnMidBeat += HandleMidBeat;
            AudioBeatDetector.Instance.OnHighBeat += HandleHighBeat;
            AudioBeatDetector.Instance.OnSubLowBeat += HandleSubLowBeat;
        }

        while (true)
        {
            yield return null;
        }
    }

    private void FireCustomBullet(EnemyBullet prefab, Vector2 spawnPos, float angle, float speed)
    {
        if (prefab == null || BulletPool.Instance == null) return;

        float rad = angle * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;

        EnemyBullet bullet = BulletPool.Instance.GetBullet(
            prefab, spawnPos, Quaternion.identity, direction * speed
        );

        if (bullet != null)
        {
            bullet.SetRotationByVelocity();
        }
    }


    private void HandleLowBeat()
    {
        if (_activeTurret == null || lowBulletPrefab == null) return;

        float step = 360f / ringBulletCount;
        float randomOffset = Random.Range(0f, step);

        for (int i = 0; i < ringBulletCount; i++)
        {
            float angle = (i * step) + randomOffset;
            FireCustomBullet(lowBulletPrefab, _activeTurret.transform.position, angle, ringSpeed);
        }
    }

    private void HandleMidBeat()
    {
        if (_activeTurret == null || midBulletPrefab == null) return;

        int sides = Random.Range(3, 7);
        float step = 360f / sides;

        float randomBaseAngle = Random.Range(0f, 360f);

        for (int i = 0; i < sides; i++)
        {
            float angle = randomBaseAngle + (i * step);

            FireCustomBullet(midBulletPrefab, _activeTurret.transform.position, angle, midBulletSpeed);
            FireCustomBullet(midBulletPrefab, _activeTurret.transform.position, angle - 15f, midBulletSpeed * 0.75f);
            FireCustomBullet(midBulletPrefab, _activeTurret.transform.position, angle + 15f, midBulletSpeed * 0.75f);
        }
    }

    private void HandleHighBeat()
    {
        if (_activeTurret == null || highBulletPrefab == null || BulletPool.Instance == null) return;

        float randomX = Random.Range(-highSpawnXRange, highSpawnXRange);
        Vector2 spawnPos = new Vector2(randomX, highSpawnY);

        FireCustomBullet(highBulletPrefab, spawnPos, 270f, highBulletSpeed);
    }

    private void HandleSubLowBeat()
    {
        if (_activeTurret == null || subLowBulletPrefab == null) return;

        float step = 360f / subLowBulletCount;
        for (int i = 0; i < subLowBulletCount; i++)
        {
            FireCustomBullet(subLowBulletPrefab, _activeTurret.transform.position, i * step, subLowSpeed);
        }
    }

    public override void OnStopped(BossTurret turret)
    {
        if (AudioBeatDetector.Instance != null)
        {
            AudioBeatDetector.Instance.OnLowBeat -= HandleLowBeat;
            AudioBeatDetector.Instance.OnMidBeat -= HandleMidBeat;
            AudioBeatDetector.Instance.OnHighBeat -= HandleHighBeat;
            AudioBeatDetector.Instance.OnSubLowBeat -= HandleSubLowBeat;
        }

        _activeTurret = null;
    }
}