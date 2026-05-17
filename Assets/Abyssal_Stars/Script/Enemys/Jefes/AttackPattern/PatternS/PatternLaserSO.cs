using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "New Laser", menuName = "Boss Patterns/Attack/Laser")]
public class PatternLaserSO : AttackPatternSO
{
    [Header("Barrido")]
    public float startAngle = 210f;
    public float endAngle = 330f;
    public float sweepSpeed = 40f;
    public bool alternateSweep = true;

    [Header("Disparo")]
    public float fireRate = 0.02f;
    public float bulletSpeed = 10f;
    public float edgePause = 0.4f;

    [Header("Huecos de Escape")]
    [Tooltip("Cuántos huecos se abren por pasada.")]
    [Min(1)]
    public int gapsPerSweep = 1;

    [Tooltip("Duración del hueco en segundos. 0.4-0.6s da tiempo justo para pasar.")]
    public float gapDuration = 0.5f;

    [Tooltip("Si está activo, el laser se ralentiza antes del hueco para avisarte.")]
    public bool telegraphGap = true;

    [Tooltip("Factor de velocidad durante el telegraph (0.35 = 35% de velocidad normal).")]
    [Range(0.1f, 0.9f)]
    public float telegraphSpeedFactor = 0.35f;

    [Tooltip("Duración del telegraph antes del hueco.")]
    public float telegraphDuration = 0.25f;

    private float _currentAngle;
    private float _direction;
    private float _fireTimer;

    public override IEnumerator ExecutePattern(BossTurret turret)
    {
        _currentAngle = startAngle;
        _direction = 1f;
        _fireTimer = 0f;

        while (true)
        {
            yield return DoSweep(turret);
            yield return new WaitForSeconds(edgePause);

            if (alternateSweep) _direction = -_direction;
            else _currentAngle = startAngle;
        }
    }

    private IEnumerator DoSweep(BossTurret turret)
    {
        float sweepRange = Mathf.Abs(endAngle - startAngle);
        float gapSpacing = sweepRange / (gapsPerSweep + 1);
        float startPos = _currentAngle;
        float distanceSince = 0f;
        int gapsFired = 0;
        float nextGapAt = gapSpacing;

        bool inGap = false;
        float gapTimer = 0f;
        bool inTelegraph = false;
        float telegraphTimer = 0f;

        while (true)
        {
            float currentSpeed = sweepSpeed;

            if (telegraphGap && !inGap && !inTelegraph && gapsFired < gapsPerSweep)
            {
                float distToGap = nextGapAt - distanceSince;
                if (distToGap <= sweepSpeed * telegraphDuration)
                {
                    inTelegraph = true;
                    telegraphTimer = 0f;
                }
            }

            if (inTelegraph)
            {
                telegraphTimer += Time.deltaTime;
                currentSpeed = sweepSpeed * telegraphSpeedFactor;

                if (telegraphTimer >= telegraphDuration)
                {
                    inTelegraph = false;
                    inGap = true;
                    gapTimer = 0f;
                }
            }

            if (inGap)
            {
                gapTimer += Time.deltaTime;
                _currentAngle += _direction * currentSpeed * Time.deltaTime;

                if (gapTimer >= gapDuration)
                {
                    inGap = false;
                    gapsFired++;
                    nextGapAt = distanceSince + gapSpacing;
                }

                yield return null;
                continue;
            }

            _currentAngle += _direction * currentSpeed * Time.deltaTime;
            distanceSince = Mathf.Abs(_currentAngle - startPos);
            _fireTimer += Time.deltaTime;

            if (_fireTimer >= fireRate)
            {
                turret.FireSingleBullet(ApplyMirror(_currentAngle), bulletSpeed);
                _fireTimer = 0f;
            }

            bool reachedEnd = _direction > 0
                ? _currentAngle >= endAngle
                : _currentAngle <= startAngle;

            if (reachedEnd)
            {
                _currentAngle = _direction > 0 ? endAngle : startAngle;
                yield break;
            }

            yield return null;
        }
    }
}