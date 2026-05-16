using System.Collections;
using UnityEngine;

public class CircularEnemy : EnemyBase
{
    [Header("Movimiento")]
    [SerializeField] private float _descendSpeed = 2f;
    [SerializeField] private float _descendDuration = 1.5f;

    [Header("Orbita")]
    [SerializeField] private float _orbitSpeed = 180f;
    [SerializeField] private float _orbitRadius = 1.2f;

    [Header("Ataque - Abanico Latigo")]
    [SerializeField] private EnemyBullet _bulletPrefab;
    [SerializeField] private int _bulletCount = 7;
    [SerializeField] private float _fanAngle = 100f;
    [SerializeField] private float _timeBetweenBullets = 0.05f;
    [SerializeField] private float _bulletSpeed = 6f;
    [SerializeField] private float _delayBetweenFans = 1.5f;

    [Header("Comportamiento General")]
    [SerializeField] private int _attackCycles = 3;

    private float _currentAngle = 0f;
    private Vector3 _localOrbitCenter;

    protected override void OnEnable()
    {
        base.OnEnable();
        _currentAngle = 0f;
        StopAllCoroutines();
        StartCoroutine(BehaviourLoop());
    }

    IEnumerator BehaviourLoop()
    {
        // 1. Fase de entrada
        yield return StartCoroutine(DescendPhase());

        // 2. Iniciamos el disparo en una corrutina SEPARADA 
        // para que no detenga el movimiento de órbita.
        Coroutine shootingRoutine = StartCoroutine(KeepFiringRoutine());

        // 3. Fase de Órbita: Se queda dando vueltas mientras la otra corrutina dispara
        float orbitTime = (_delayBetweenFans + (_bulletCount * _timeBetweenBullets)) * _attackCycles;
        float elapsed = 0f;

        while (elapsed < orbitTime)
        {
            UpdateOrbitPosition();
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 4. Detenemos los disparos y nos vamos
        StopCoroutine(shootingRoutine);
        yield return StartCoroutine(ExitPhase());
    }

    // --- Lógica de Órbita (Separada para reusarla) ---
    void UpdateOrbitPosition()
    {
        _currentAngle += _orbitSpeed * Time.deltaTime;
        float rad = _currentAngle * Mathf.Deg2Rad;

        Vector3 offset = new Vector3(
            Mathf.Cos(rad) * _orbitRadius,
            Mathf.Sin(rad) * _orbitRadius,
            0f
        );

        transform.localPosition = _localOrbitCenter + offset;
    }

    IEnumerator DescendPhase()
    {
        float elapsed = 0f;
        while (elapsed < _descendDuration)
        {
            transform.localPosition += Vector3.down * _descendSpeed * Time.deltaTime;
            elapsed += Time.deltaTime;
            yield return null;
        }
        // Guardamos el centro final una vez que termina de bajar
        _localOrbitCenter = transform.localPosition;
    }

    // --- Esta corrutina corre en paralelo al movimiento ---
    IEnumerator KeepFiringRoutine()
    {
        while (true)
        {
            float startAngle = -90f - (_fanAngle / 2f);
            float angleStep = _bulletCount > 1 ? _fanAngle / (_bulletCount - 1) : 0f;

            for (int i = 0; i < _bulletCount; i++)
            {
                float angle = startAngle + angleStep * i;
                Vector3 dir = AngleToDirection(angle); 

                SpawnBullet(angle, dir);

                yield return new WaitForSeconds(_timeBetweenBullets);
            }

            yield return new WaitForSeconds(_delayBetweenFans);
        }
    }

    void SpawnBullet(float exactAngle, Vector3 direction)
    {
        if (_bulletPrefab == null) return;

        // Usamos el exactAngle directamente, sin calcular Atan2
        BulletPool.Instance.GetBullet(
            _bulletPrefab,
            transform.position,
            Quaternion.Euler(0, 0, exactAngle),
            direction * _bulletSpeed
        );
    }

    IEnumerator ExitPhase()
    {
        while (transform.localPosition.y > -12f)
        {
            _localOrbitCenter += Vector3.down * _descendSpeed * 2f * Time.deltaTime;

            UpdateOrbitPosition();

            yield return null;
        }
        ReturnToPool();
    }

    // --- Utilidades de disparo ---
    Vector3 AngleToDirection(float angleDeg) => new Vector3(Mathf.Cos(angleDeg * Mathf.Deg2Rad), Mathf.Sin(angleDeg * Mathf.Deg2Rad), 0f);

    void SpawnBullet(Vector3 direction)
    {
        if (_bulletPrefab == null) return;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        BulletPool.Instance.GetBullet(_bulletPrefab, transform.position, Quaternion.AngleAxis(angle, Vector3.forward), direction * _bulletSpeed);
    }

}