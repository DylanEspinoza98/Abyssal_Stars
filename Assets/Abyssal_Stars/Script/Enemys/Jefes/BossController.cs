using UnityEngine;
using System.Collections;

public class BossController : EnemyBase
{

    [Header("Torretas (orden importante — coincide con turretPatterns en cada fase)")]
    [SerializeField] private TurretAssignment[] _turrets;

    [Header("Fases")]
    [SerializeField] private BossPhaseSO[] _phases;

    [Header("Entrada")]
    [SerializeField] private Vector2 _zoneCenter = new Vector2(0f, 2.5f);
    [SerializeField] private float _entrySpeed = 2f;

    private bool _isDying = false;
    private Coroutine _phaseLoop;
    private Coroutine _activeMovement;
    private Camera _cam;

    protected override void OnEnable()
    {
        base.OnEnable();

        _cam = Camera.main;
        _isDying = false;

        if (transform.parent == null && _cam != null)
            transform.SetParent(_cam.transform);

        transform.localRotation = Quaternion.Euler(0f, 0f, 180f);

        SetAllTurrets(active: false);
        StartCoroutine(EnterAndStartPhases());
    }

    protected override void Update()
    {
        if (_isDying) return;
        base.Update();
    }
    private IEnumerator EnterAndStartPhases()
    {
        Vector3 target = new Vector3(_zoneCenter.x, _zoneCenter.y, 10f);

        while (Vector3.Distance(transform.localPosition, target) > 0.1f)
        {
            transform.localPosition = Vector3.MoveTowards(
                transform.localPosition, target, _entrySpeed * Time.deltaTime
            );
            yield return null;
        }

        _phaseLoop = StartCoroutine(CyclePhasesRoutine());
    }

    private IEnumerator CyclePhasesRoutine()
    {
        if (_phases == null || _phases.Length == 0) yield break;

        int index = 0;

        while (!_isDying)
        {
            BossPhaseSO phase = _phases[index];

            if (phase == null)
            {
                index = (index + 1) % _phases.Length;
                continue;
            }

            // Aplicar torretas de esta fase
            ApplyTurrets(phase);

            // Movimiento
            if (phase.movementPattern != null)
                _activeMovement = StartCoroutine(
                    phase.movementPattern.ExecuteMovement(transform, _zoneCenter)
                );

            yield return new WaitForSeconds(phase.duration);

            StopActiveMovement();
            StopAllTurrets();

            if (phase.transitionDelay > 0f)
                yield return new WaitForSeconds(phase.transitionDelay);

            index = (index + 1) % _phases.Length;
        }
    }

    private void ApplyTurrets(BossPhaseSO phase)
    {
        if (_turrets == null) return;


        for (int i = 0; i < _turrets.Length; i++)
        {
            BossTurret turret = _turrets[i]?.turret;
            if (turret == null)
            {
                continue;
            }

            bool hasPattern = phase.turretPatterns != null
                              && i < phase.turretPatterns.Length
                              && phase.turretPatterns[i] != null;

            if (hasPattern)
            {
                turret.gameObject.SetActive(true);
                turret.RunPattern(phase.turretPatterns[i], phase.duration);
            }
            else
            {
                turret.StopCurrentPattern();
                turret.gameObject.SetActive(false);
            }
        }
    }

    private void StopAllTurrets()
    {
        if (_turrets == null) return;
        foreach (TurretAssignment t in _turrets)
            t?.turret?.StopCurrentPattern();
    }

    private void SetAllTurrets(bool active)
    {
        if (_turrets == null) return;
        foreach (TurretAssignment t in _turrets)
        {
            if (t?.turret == null) continue;
            t.turret.StopCurrentPattern();
            t.turret.gameObject.SetActive(active);
        }
    }
    protected override void Die()
    {
        if (_isDying) return;
        StartCoroutine(TheatricalDeathRoutine());
    }

    private IEnumerator TheatricalDeathRoutine()
    {
        _isDying = true;

        StopActiveMovement();
        SetAllTurrets(active: false);

        if (_phaseLoop != null) { StopCoroutine(_phaseLoop); _phaseLoop = null; }

        AudioBeatDetector.Instance?.StopMusic();

        for (int i = 0; i < 6; i++)
        {
            if (_explosionEffectPrefab != null)
            {
                Vector3 offset = new Vector3(
                    Random.Range(-2f, 2f), Random.Range(-1.5f, 1.5f), 0f
                );
                Instantiate(_explosionEffectPrefab, transform.position + offset, Quaternion.identity);
            }
            yield return new WaitForSeconds(Mathf.Max(0.05f, 0.4f - i * 0.05f));
        }

        VictoryManager.Instance?.ShowVictory();
        base.Die();
    }

    private void StopActiveMovement()
    {
        if (_activeMovement != null)
        {
            StopCoroutine(_activeMovement);
            _activeMovement = null;
        }
    }
}