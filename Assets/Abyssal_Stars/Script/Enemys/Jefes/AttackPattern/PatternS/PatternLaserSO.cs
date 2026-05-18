using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "New Laser", menuName = "Boss Patterns/Attack/Laser")]
public class PatternLaserSO : AttackPatternSO
{
    [Header("Barrido")]
    public float startAngle = 200f;
    public float endAngle = 300f;
    public float sweepSpeed = 40f;
    public bool alternateSweep = true;

    [Header("Disparo")]
    public float fireRate = 0.02f;
    public float bulletSpeed = 10f;

    [Header("Huecos de Escape (Gaps)")]
    [Tooltip("Cuántos huecos por barrido. 1 = justo al centro, 2 = divididos simétricamente, etc.")]
    public int gapsPerSweep = 1;
    public float gapDuration = 0.5f;

    [Header("Aviso Visual (Telegraph)")]
    public bool telegraphGap = true;
    public float telegraphDuration = 0.25f;
    [Range(0.1f, 0.9f)] public float telegraphSpeedFactor = 0.35f;

    private float _currentAngle;
    private float _direction;

    public override IEnumerator ExecutePattern(BossTurret turret)
    {
        _currentAngle = startAngle;
        _direction = 1f;

        while (true)
        {
            yield return DoSweep(turret);

            if (alternateSweep)
                _direction = -_direction;
            else
                _currentAngle = startAngle;
        }
    }

    private IEnumerator DoSweep(BossTurret turret)
    {
        float totalSweepAngle = Mathf.Abs(endAngle - startAngle);
        float anglePerSegment = totalSweepAngle / (gapsPerSweep + 1);

        float gapDegrees = sweepSpeed * gapDuration;
        float telegraphDegrees = telegraphGap ? (sweepSpeed * telegraphSpeedFactor) * telegraphDuration : 0f;

        float sweptAngle = 0f;
        int gapsDone = 0;
        float fireTimer = 0f;

        bool inGap = false;
        bool inTelegraph = false;
        float stateTimer = 0f;

        while (true)
        {
            float currentSpeed = sweepSpeed;

            if (!inGap && !inTelegraph && gapsDone < gapsPerSweep)
            {
                float targetAngle = anglePerSegment * (gapsDone + 1);

                float triggerAngle = targetAngle - (gapDegrees / 2f) - telegraphDegrees;
                triggerAngle = Mathf.Max(0f, triggerAngle);

                if (sweptAngle >= triggerAngle)
                {
                    if (telegraphGap) inTelegraph = true;
                    else inGap = true;
                    stateTimer = 0f;
                }
            }

            if (inTelegraph)
            {
                currentSpeed = sweepSpeed * telegraphSpeedFactor;
                stateTimer += Time.deltaTime;

                if (stateTimer >= telegraphDuration)
                {
                    inTelegraph = false;
                    inGap = true;
                    stateTimer = 0f;
                }
            }
            else if (inGap)
            {
                stateTimer += Time.deltaTime;
                if (stateTimer >= gapDuration)
                {
                    inGap = false;
                    gapsDone++;
                }
            }

            float moveStep = currentSpeed * Time.deltaTime;
            _currentAngle += _direction * moveStep;
            sweptAngle += moveStep;

            if (!inGap)
            {
                fireTimer += Time.deltaTime;
                if (fireTimer >= fireRate)
                {
                    turret.FireSingleBullet(ApplyMirror(_currentAngle), bulletSpeed);
                    fireTimer = 0f;
                }
            }

            if (ReachedEnd())
            {
                ClampToEnd();
                yield break;
            }

            yield return null;
        }
    }

    private bool ReachedEnd()
    {
        return _direction > 0 ? _currentAngle >= endAngle : _currentAngle <= startAngle;
    }

    private void ClampToEnd()
    {
        _currentAngle = _direction > 0 ? endAngle : startAngle;
    }

}