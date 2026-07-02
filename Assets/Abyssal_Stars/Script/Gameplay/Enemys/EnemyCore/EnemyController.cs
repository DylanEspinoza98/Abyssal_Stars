using UnityEngine;
using System.Collections;

public class EnemyController : EnemyBase
{
    [Header("Armamento")]
    [SerializeField] private TurretAssignment[] _turrets;

    [Header("Fases del Enemigo")]
    [SerializeField] private BossPhaseSO[] _phases;

    [Tooltip("True: Repite las fases indefinidamente (Mini-Jefes). False: Se destruye al terminar la última fase.")]
    [SerializeField] private bool _loopPhases = false;

    [Header("Entrada")]
    [Tooltip("Unidades que desciende antes de empezar a atacar.")]
    [SerializeField] private float _entryDistance = 3f;

    [Tooltip("Velocidad de descenso en la entrada.")]
    [SerializeField] private float _entrySpeed = 4f;

    private Coroutine _phaseLoop;
    private Coroutine _activeMovement;
    private Vector2 _startCenter;

    protected override void OnEnable()
    {
        base.OnEnable();

        SetAllTurrets(active: false);

        if (_phases == null || _phases.Length == 0)
        {
            Debug.LogError($"[EnemyController] {gameObject.name} no tiene fases asignadas.");
            return;
        }

        _phaseLoop = StartCoroutine(EntryThenExecute());
    }

    private IEnumerator EntryThenExecute()
    {
        Vector3 entryTarget = transform.position + Vector3.down * _entryDistance;

        while (Vector3.Distance(transform.position, entryTarget) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position, entryTarget, _entrySpeed * Time.deltaTime
            );
            yield return null;
        }

        _startCenter = transform.position;

        yield return StartCoroutine(ExecutePhaseSequence());
    }

    private IEnumerator ExecutePhaseSequence()
    {
        int index = 0;

        while (gameObject.activeInHierarchy && !_isRetreating)
        {
            BossPhaseSO phase = _phases[index];

            if (phase != null)
            {
                ApplyTurrets(phase);
                ExecuteMovement(phase);

                yield return new WaitForSeconds(phase.Duration);

                StopCurrentActivities();

                if (phase.TransitionDelay > 0f)
                    yield return new WaitForSeconds(phase.TransitionDelay);
            }

            index++;

            if (index >= _phases.Length)
            {
                if (_loopPhases)
                    index = 0;
                else
                {
                    ReturnToPool();
                    yield break;
                }
            }
        }
    }

    private void ExecuteMovement(BossPhaseSO phase)
    {
        if (phase.movementPattern != null)
            _activeMovement = StartCoroutine(
                phase.movementPattern.ExecuteMovement(transform, _startCenter)
            );
    }

    private void ApplyTurrets(BossPhaseSO phase)
    {
        if (_turrets == null) return;

        for (int i = 0; i < _turrets.Length; i++)
        {
            BossTurret turret = _turrets[i]?.turret;
            if (turret == null) continue;

            bool hasPattern = phase.turretPatterns != null
                              && i < phase.turretPatterns.Length
                              && phase.turretPatterns[i] != null;

            if (hasPattern)
            {
                turret.gameObject.SetActive(true);
                turret.RunPattern(phase.turretPatterns[i], phase.Duration);
            }
            else
            {
                turret.StopCurrentPattern();
                turret.gameObject.SetActive(false);
            }
        }
    }

    private void StopCurrentActivities()
    {
        if (_activeMovement != null)
        {
            StopCoroutine(_activeMovement);
            _activeMovement = null;
        }
        SetAllTurrets(active: false);
    }

    private void SetAllTurrets(bool active)
    {
        if (_turrets == null) return;

        foreach (TurretAssignment assignment in _turrets)
        {
            if (assignment?.turret == null) continue;
            assignment.turret.StopCurrentPattern();
            assignment.turret.gameObject.SetActive(active);
        }
    }

    private void HaltAllProcesses()
    {
        if (_phaseLoop != null)
        {
            StopCoroutine(_phaseLoop);
            _phaseLoop = null;
        }
        StopCurrentActivities();
    }
    protected override void Die()
    {
        HaltAllProcesses();
        base.Die();
    }

    public override void ReturnToPool()
    {
        HaltAllProcesses();
        base.ReturnToPool();
    }
}