using UnityEngine;
using System.Collections;

public class EnemyShield : MonoBehaviour
{
    [Header("Durabilidad")]
    [Tooltip("Golpes que absorbe antes de romperse. 0 = indestructible.")]
    [SerializeField] private int _maxHits = 0;

    [Header("Feedback - Flash de Color")]
    [Tooltip("Color del flash al recibir un impacto. Se restaura al terminar.")]
    [SerializeField] private Color _hitColor = Color.white;

    [Header("Feedback - Pulso de Escala")]
    [Tooltip("Escala máxima del pulso. 1.3 = crece un 30% al impacto.")]
    [SerializeField] private float _pulseScale = 1.3f;
    [Tooltip("Duración total del pulso (expansión + contracción).")]
    [SerializeField] private float _pulseDuration = 0.25f;

    private int _currentHits;
    private SpriteRenderer _sr;
    private Color _originalColor;
    private Vector3 _originalLocalScale;
    private Coroutine _hitRoutine;

    private EnemyBase _creator;

    private void Awake()
    {
        _sr = GetComponent<SpriteRenderer>();
        if (_sr != null) _originalColor = _sr.color;
    }

    private void OnEnable()
    {
        _currentHits = 0;
        if (_sr != null) _sr.color = _originalColor;

    }

    public void Initialize(EnemyBase creator)
    {
        _creator = creator;

        _originalLocalScale = transform.localScale;
    }

    private void Update()
    {
        if (_creator == null || !_creator.gameObject.activeInHierarchy)
        {
            Dissolve();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("PlayerBullet")) return;

        if (!gameObject.activeInHierarchy) return;

        if (_hitRoutine != null) StopCoroutine(_hitRoutine);

        if (_maxHits > 0)
        {
            _currentHits++;
            if (_currentHits >= _maxHits)
            {
                Dissolve();
                return;
            }
        }

        _hitRoutine = StartCoroutine(HitEffect());
    }

    private IEnumerator HitEffect()
    {
        float expandTime = _pulseDuration * 0.3f;
        float contractTime = _pulseDuration * 0.7f;

        if (_sr != null) _sr.color = _hitColor;

        float elapsed = 0f;
        while (elapsed < expandTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / expandTime);
            transform.localScale = _originalLocalScale * Mathf.Lerp(1f, _pulseScale, t);
            yield return null;
        }

        if (_sr != null) _sr.color = _originalColor;

        elapsed = 0f;
        while (elapsed < contractTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / contractTime);
            float easedT = 1f - (1f - t) * (1f - t);
            transform.localScale = _originalLocalScale * Mathf.Lerp(_pulseScale, 1f, easedT);
            yield return null;
        }

        transform.localScale = _originalLocalScale;
        _hitRoutine = null;
    }

    public void Dissolve()
    {
        if (_hitRoutine != null) StopCoroutine(_hitRoutine);
        Destroy(gameObject);
    }
}