using UnityEngine;
using System.Collections;

public class BossTurret : MonoBehaviour
{
    [Header("Munici�n")]
    [SerializeField] private EnemyBullet _bulletPrefab;
    public EnemyBullet BulletPrefab => _bulletPrefab;

    [SerializeField] private Sprite _bulletSprite;
    public Sprite BulletSprite => _bulletSprite;

    [SerializeField] private Color _bulletColor = Color.white;
    public Color BulletColor => _bulletColor;

    [Header("Rotaci�n de Sprite")]
    [Tooltip("Si est� activo, el sprite de la torreta gira hacia donde dispara.")]
    [SerializeField] private bool _rotateToFireDirection = true;

    [Tooltip("Offset de rotaci�n para corregir la orientaci�n base del sprite.")]
    [SerializeField] private float _rotationOffset = 90f;

    [Tooltip("Velocidad de rotaci�n (0 = instant�neo, mayor = m�s suave).")]
    [SerializeField] private float _rotationSpeed = 0f;

    [Header("Ciclo de Patrones Aut�nomo")]
    [Tooltip("Solo si esta torreta corre sus patrones de forma independiente.")]
    [SerializeField] private AttackPatternSO[] _patternPlaylist;
    [SerializeField] private float _timePerPattern = 5f;
    [SerializeField] private float _transitionDelay = 1f;

    [Header("Componentes Especiales (Opcionales)")]
    [Tooltip("Asign� esto solo si esta torreta usar� ataques de l�ser continuo.")]
    [SerializeField] private LineRenderer _laserLineRenderer;
    public LineRenderer LaserLineRenderer => _laserLineRenderer;

    [Tooltip("Punto de origen exacto del l�ser. Si est� vac�o, usa el transform de la torreta.")]
    [SerializeField] private Transform _laserFirePoint;
    public Transform LaserFirePoint => _laserFirePoint;

    [Tooltip("Asign� esto si el ataque altera el color del sprite (ej. Sobrecalentamiento).")]
    [SerializeField] private SpriteRenderer _turretSpriteRenderer;
    public SpriteRenderer TurretSpriteRenderer => _turretSpriteRenderer;

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
        if (!_rotateToFireDirection) return;

        if (_rotationSpeed <= 0f)
            transform.rotation = _targetRotation;
        else
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, _targetRotation, _rotationSpeed * Time.deltaTime
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

        if (_laserLineRenderer != null && _laserLineRenderer.enabled)
            _laserLineRenderer.enabled = false;

        ReturnOrbitalBullets();
    }

    public void ResetVisuals()
    {
        if (_turretSpriteRenderer != null)
            _turretSpriteRenderer.color = Color.white;

        if (_laserLineRenderer != null)
            _laserLineRenderer.enabled = false;
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
        if (_bulletPrefab == null || BulletPool.Instance == null) return;

        float rad = exactAngle * Mathf.Deg2Rad;
        Vector2 direction = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad)).normalized;

        if (_rotateToFireDirection)
            RotateToAngle(exactAngle);

        EnemyBullet bullet = BulletPool.Instance.GetBullet(
            _bulletPrefab, transform.position, Quaternion.identity, direction * speed
        );

        if (bullet == null) return;

        bullet.SetRotationByVelocity();
        if (_bulletSprite != null) bullet.SetAppearance(_bulletSprite, _bulletColor);
    }

    /// <summary>
    /// Permite que un patrón tome control total de la rotación desactivando
    /// el sistema automático de BossTurret. Llamar con false al iniciar el
    /// patrón y con true en OnStopped para restaurar.
    /// </summary>
    public void EnableAutoRotation(bool enabled)
    {
        _rotateToFireDirection = enabled;
    }

    public void RotateToAngle(float angle)
    {
        float corrected = angle + _rotationOffset;
        _targetRotation = Quaternion.Euler(0f, 0f, corrected);

        if (_rotationSpeed <= 0f)
            transform.rotation = _targetRotation;
    }

    public EnemyBullet SpawnBulletWithoutFiring()
    {
        if (_bulletPrefab == null || BulletPool.Instance == null) return null;

        EnemyBullet bullet = BulletPool.Instance.GetBullet(
            _bulletPrefab, transform.position, Quaternion.identity, Vector2.zero
        );

        if (bullet == null) return null;
        if (_bulletSprite != null) bullet.SetAppearance(_bulletSprite, _bulletColor);

        return bullet;
    }
}
