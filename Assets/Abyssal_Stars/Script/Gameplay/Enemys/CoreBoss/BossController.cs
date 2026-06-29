using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

// BossPhaseThreshold
/* Define un umbral de HP en el que el jefe interrumpe su patrón actual,
   ejecuta un ataque especial de transición y reemplaza su rotación de fases. */
[Serializable]
public class BossPhaseThreshold
{
    [Tooltip("% de vida al que se activa este umbral. Ej: 0.6 = al bajar del 60%.")]
    [Range(0.01f, 0.99f)]
    public float healthPercent = 0.5f;

    [Tooltip("Fase especial (BossPhaseSO) que se ejecuta UNA SOLA VEZ como transición (ej: ir al fondo).")]
    public BossPhaseSO transitionPhase;

    [Tooltip("Fases NUEVAS que se sumarán automáticamente a la rotación actual después de la transición.")]
    public BossPhaseSO[] phasesToAdd;

    [HideInInspector] public bool triggered;
}

// BossController
public class BossController : EnemyBase
{
    public static event Action OnBossDefeated;

    [Header("Torretas (orden coincide con turretPatterns en cada fase)")]
    [SerializeField] private TurretAssignment[] _turrets;

    [Header("Fases Base")]
    [SerializeField] private BossPhaseSO[] _phases;

    [Header("Umbrales de HP")]
    [Tooltip("El orden en el Inspector no importa: se ordenan automáticamente de mayor a menor HP%.")]
    [SerializeField] private BossPhaseThreshold[] _thresholds;

    [Header("Entrada")]
    [SerializeField] private Vector2 _zoneCenter = new Vector2(0f, 2.5f);
    [SerializeField] private float _entrySpeed = 2f;

    // Estado interno
    private BossPhaseSO[] _activePhases;
    private bool _isDying = false;
    private bool _inTransition = false;
    private Coroutine _phaseLoop;
    private Coroutine _activeMovement;
    private MovementPatternSO _activeMovementPattern;
    private Coroutine _transitionCoroutine;
    private Camera _cam;
    private bool _transitionPending = false;

    // Ciclo de vida

    protected override void OnEnable()
    {
        base.OnEnable();

        _cam = Camera.main;
        _isDying = false;
        _inTransition = false;
        _activePhases = _phases;

        if (_thresholds != null)
        {
            foreach (BossPhaseThreshold t in _thresholds)
                t.triggered = false;
            Array.Sort(_thresholds, (a, b) => b.healthPercent.CompareTo(a.healthPercent));
        }

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

    // Sistema de umbrales de HP del Boss

    protected override void OnTookDamage()
    {
        if (_isDying || _inTransition || _transitionPending) return;

        BossPhaseThreshold pending = GetPendingThreshold();
        if (pending != null)
        {
            _transitionPending = true;
        }
    }

    // Devuelve el primer umbral no disparado cuyo HP% supera la vida actual.
    private BossPhaseThreshold GetPendingThreshold()
    {
        if (_thresholds == null) return null;

        float hp = HealthPercent;
        foreach (BossPhaseThreshold t in _thresholds)
        {
            if (!t.triggered && hp <= t.healthPercent)
                return t;
        }
        return null;
    }

    private IEnumerator ExecuteTransition(BossPhaseThreshold threshold)
    {
        _inTransition = true;

        // Interrumpir limpiamente el ciclo actual
        if (_phaseLoop != null) { StopCoroutine(_phaseLoop); _phaseLoop = null; }
        StopActiveMovement();
        StopAllTurrets();

        // Ejecutar la Fase de Transición (misma lógica que las fases normales)
        if (threshold.transitionPhase != null)
        {
            ApplyTurrets(threshold.transitionPhase);

            if (threshold.transitionPhase.movementPattern != null)
            {
                _activeMovementPattern = threshold.transitionPhase.movementPattern;  // << NUEVO
                _activeMovement = StartCoroutine(
                    _activeMovementPattern.ExecuteMovement(transform, _zoneCenter)
                );
            }

            yield return new WaitForSeconds(threshold.transitionPhase.duration);

            StopActiveMovement();
            StopAllTurrets();

            if (threshold.transitionPhase.transitionDelay > 0f)
                yield return new WaitForSeconds(threshold.transitionPhase.transitionDelay);
        }

        // Sumar las nuevas fases a la rotación sin perder las anteriores
        if (threshold.phasesToAdd != null && threshold.phasesToAdd.Length > 0)
        {
            List<BossPhaseSO> updatedPhases = new List<BossPhaseSO>(_activePhases);
            updatedPhases.AddRange(threshold.phasesToAdd);
            _activePhases = updatedPhases.ToArray();
        }

        _transitionCoroutine = null;
        _inTransition = false;

        // Revisar si hay que encadenar otro umbral o volver al ciclo normal
        BossPhaseThreshold chained = GetPendingThreshold();
        if (chained != null)
        {
            chained.triggered = true;
            _transitionCoroutine = StartCoroutine(ExecuteTransition(chained));
        }
        else
        {
            _phaseLoop = StartCoroutine(CyclePhasesRoutine());
        }
    }

    // Ciclo de fases

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
        if (_activePhases == null || _activePhases.Length == 0) yield break;

        int index = 0;

        while (!_isDying)
        {
            // Verificación de Transición Pendiente
            if (_transitionPending)
            {
                _transitionPending = false;
                BossPhaseThreshold next = GetPendingThreshold();
                if (next != null)
                {
                    next.triggered = true;
                    yield return StartCoroutine(ExecuteTransition(next));
                    yield break;
                }
            }

            // Ejecución normal de fase
            BossPhaseSO phase = _activePhases[index];

            if (phase == null)
            {
                index = (index + 1) % _activePhases.Length;
                continue;
            }

            ApplyTurrets(phase);

            if (phase.movementPattern != null)
            {
                _activeMovementPattern = phase.movementPattern;                     
                _activeMovement = StartCoroutine(
                    _activeMovementPattern.ExecuteMovement(transform, _zoneCenter)
                );
            }

            yield return new WaitForSeconds(phase.duration);

            StopActiveMovement();
            StopAllTurrets();

            if (phase.transitionDelay > 0f)
                yield return new WaitForSeconds(phase.transitionDelay);

            index = (index + 1) % _activePhases.Length;
        }
    }

    // Muerte

    protected override void Die()
    {
        if (_isDying) return;
        StartCoroutine(TheatricalDeathRoutine());
    }

    private IEnumerator TheatricalDeathRoutine()
    {
        _isDying = true;

        if (_transitionCoroutine != null) { StopCoroutine(_transitionCoroutine); _transitionCoroutine = null; }
        if (_phaseLoop != null) { StopCoroutine(_phaseLoop); _phaseLoop = null; }
        StopActiveMovement();
        SetAllTurrets(active: false);

        AudioBeatDetector.Instance?.StopMusic();

        for (int i = 0; i < 6; i++)
        {
            if (_explosionEffectPrefab != null)
            {
                Vector3 offset = new Vector3(
                    UnityEngine.Random.Range(-2f, 2f),
                    UnityEngine.Random.Range(-1.5f, 1.5f),
                    0f
                );
                Instantiate(_explosionEffectPrefab, transform.position + offset, Quaternion.identity);
            }
            yield return new WaitForSeconds(Mathf.Max(0.05f, 0.4f - i * 0.05f));
        }

        OnBossDefeated?.Invoke();

        base.Die();
    }

    // Helpers de torretas

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

    private void StopActiveMovement()
    {
        if (_activeMovement != null)
        {
            StopCoroutine(_activeMovement);
            _activeMovement = null;
        }

        // Dar oportunidad al patrón de limpiar sus efectos secundarios
        // (ej: RetreatToDepth restaura velocidad del parallax y escala del boss)
        _activeMovementPattern?.OnStopped(transform);
        _activeMovementPattern = null;
    }
}