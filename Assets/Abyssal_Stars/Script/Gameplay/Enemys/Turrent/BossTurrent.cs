using UnityEngine;
using System.Collections;

public class BossTurret : MonoBehaviour
{
    [Header("Munición")]
    public EnemyBullet bulletPrefab;
    public Sprite bulletSprite;
    public Color bulletColor = Color.white;

    [Header("Rotación de Sprite")]
    [Tooltip("Si está activo, el sprite de la torreta gira hacia donde dispara.")]
    public bool rotateToFireDirection = true;
    [Tooltip("Offset de rotación para corregir la orientación base del sprite.")]
    public float rotationOffset = 90f;
    [Tooltip("Velocidad de rotación (0 = instantáneo, mayor = más suave).")]
    public float rotationSpeed = 0f;

    [Header("Ciclo de Patrones Autónomo")]
    [Tooltip("Solo si esta torreta corre sus patrones de forma independiente.")]
    [SerializeField] private AttackPatternSO[] _patternPlaylist;
    [SerializeField] private float _timePerPattern = 5f;
    [SerializeField] private float _transitionDelay = 1f;

    [Header("Componentes Especiales (Opcionales)")]
    [Tooltip("Asigná esto solo si esta torreta usará ataques de láser continuo.")]
    public LineRenderer laserLineRenderer;
    [Tooltip("Punto de origen exacto del láser. Si está vacío, usa el transform de la torreta.")]
    public Transform laserFirePoint;
    [Tooltip("Asigná esto si el ataque altera el color del sprite (ej. Sobrecalentamiento).")]
    public SpriteRenderer turretSpriteRenderer;

    private Coroutine _activePatternCoroutine;
    private Coroutine _activeExecuteCoroutine;
    private bool _isRunning = false;
    private AttackPatternSO _currentPattern;
    private Quaternion _targetRotation;
    private void Awake()
    {
        _targetRotation = transform.rotation;
    }

    private void Start()
    {
        if (_patternPlaylist != null && _patternPlaylist.Length > 0)
            StartAutonomousCycle();
    }

    private void OnDisable()
    {
        StopCurrentPattern();
        ResetVisuals();
    }

    private void Update()
    {
        if (!rotateToFireDirection) return;

        if (rotationSpeed <= 0f)
            transform.rotation = _targetRotation;
        else
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, _targetRotation, rotationSpeed * Time.deltaTime
            );
    }
    private void StartAutonomousCycle()
    {
        _isRunning = true;
        StartCoroutine(CyclePatternsRoutine());
    }

    private IEnumerator CyclePatternsRoutine()
    {
        int index = 0;

        while (_isRunning)
        {
            yield return RunPattern(_patternPlaylist[index], _timePerPattern);
            yield return new WaitForSeconds(_transitionDelay);
            index = (index + 1) % _patternPlaylist.Length;
        }
    }
    public Coroutine RunPattern(AttackPatternSO pattern, float duration)
    {
        StopCurrentPattern();

        _currentPattern = pattern;

        _activePatternCoroutine = StartCoroutine(RunPatternRoutine(pattern, duration));
        return _activePatternCoroutine;
    }

    public void StopCurrentPattern()
    {
        if (_currentPattern != null)
        {
            _currentPattern.OnStopped(this);
            _currentPattern = null;
        }

        if (_activeExecuteCoroutine != null)
        {
            StopCoroutine(_activeExecuteCoroutine);
            _activeExecuteCoroutine = null;
        }

        if (_activePatternCoroutine != null)
        {
            StopCoroutine(_activePatternCoroutine);
            _activePatternCoroutine = null;
        }

        if (laserLineRenderer != null && laserLineRenderer.enabled)
            laserLineRenderer.enabled = false;

        ReturnOrbitalBullets();
    }

    public void ResetVisuals()
    {
        if (turretSpriteRenderer != null)
            turretSpriteRenderer.color = Color.white;

        if (laserLineRenderer != null)
            laserLineRenderer.enabled = false;
    }

    public void ReturnOrbitalBullets()
    {
        Transform root = transform.root;
        for (int i = root.childCount - 1; i >= 0; i--)
        {
            Transform child = root.GetChild(i);
            EnemyBullet bullet = child.GetComponent<EnemyBullet>();
            if (bullet == null) continue;

            child.SetParent(null);
            bullet.gameObject.SetActive(false);
        }
    }
    private IEnumerator RunPatternRoutine(AttackPatternSO pattern, float duration)
    {
        if (pattern == null) yield break;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            bool done = false;
            _activeExecuteCoroutine = StartCoroutine(RunAndSignal(pattern, () => done = true));

            while (!done && elapsed < duration)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            if (_activeExecuteCoroutine != null)
            {
                StopCoroutine(_activeExecuteCoroutine);
                _activeExecuteCoroutine = null;
            }
        }
    }

    private IEnumerator RunAndSignal(AttackPatternSO pattern, System.Action onDone)
    {
        yield return pattern.ExecutePattern(this);
        onDone?.Invoke();
    }
    public void FireSingleBullet(float exactAngle, float speed)
    {
        if (bulletPrefab == null || BulletPool.Instance == null) return;

        float rad = exactAngle * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;

        if (rotateToFireDirection)
            RotateToAngle(exactAngle);

        EnemyBullet bullet = BulletPool.Instance.GetBullet(
            bulletPrefab, transform.position, Quaternion.identity, direction * speed
        );

        if (bullet == null) return;

        bullet.SetRotationByVelocity();
        if (bulletSprite != null) bullet.SetAppearance(bulletSprite, bulletColor);
    }

    public void RotateToAngle(float angle)
    {
        float corrected = angle + rotationOffset;
        _targetRotation = Quaternion.Euler(0f, 0f, corrected);

        if (rotationSpeed <= 0f)
            transform.rotation = _targetRotation;
    }

    public EnemyBullet SpawnBulletWithoutFiring()
    {
        if (bulletPrefab == null || BulletPool.Instance == null) return null;

        EnemyBullet bullet = BulletPool.Instance.GetBullet(
            bulletPrefab, transform.position, Quaternion.identity, Vector2.zero
        );

        if (bullet == null) return null;
        if (bulletSprite != null) bullet.SetAppearance(bulletSprite, bulletColor);

        return bullet;
    }
}