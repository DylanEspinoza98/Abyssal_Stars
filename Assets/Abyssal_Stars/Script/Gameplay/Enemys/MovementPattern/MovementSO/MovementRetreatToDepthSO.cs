using UnityEngine;
using System.Collections;

[CreateAssetMenu(fileName = "New Retreat To Depth", menuName = "Boss Patterns/Movement/Retreat To Depth")]
public class MovementRetreatToDepthSO : MovementPatternSO
{
    [Header("Fase 0: Advertencia (Telegraph)")]
    [SerializeField] private GameObject warningPrefab;
    [SerializeField] private float warningDuration = 1.2f;
    [SerializeField] private bool shakeBeforeEscape = true;
    [SerializeField] private float shakeIntensity = 0.15f;

    [Header("Limites de Pantalla (Fuera de Cámara)")]
    [SerializeField] private float offScreenBottomY = -12f;
    [SerializeField] private float offScreenTopY = 12f;

    [Header("Configuración de Fondo (El Truco)")]
    [Tooltip("Multiplicador de tamaño. Si tu jefe mide 2, y esto es 0.3, al fondo medirá 0.6.")]
    [SerializeField] private float targetScale = 0.3f;
    [SerializeField] private int backgroundSortingOrder = -20;
    [SerializeField] private float backgroundParallaxSpeed = 0.2f;

    [Header("Fase 1: El Escape")]
    [SerializeField] private float escapeSpeed = 15f;

    [Header("Fase 2: El Ascenso (Fondo)")]
    [SerializeField] private float ascendSpeed = 3f;
    [SerializeField] private float zigzagAmplitude = 1.5f;
    [SerializeField] private float zigzagSpeed = 2f;

    [Header("Fase 3: El Regreso")]
    [SerializeField] private float diveSpeed = 20f;

    // Memoria temporal
    [System.NonSerialized] private SpriteRenderer[] _renderers;
    [System.NonSerialized] private int[] _originalSortingOrders;
    [System.NonSerialized] private Vector3 _originalScale;
    [System.NonSerialized] private Collider2D _bossCollider;
    [System.NonSerialized] private string _originalTag;

    public override IEnumerator ExecuteMovement(Transform bossTransform, Vector2 zoneCenter)
    {
        // ==========================================
        // 0. GUARDAR ESTADO ORIGINAL
        // ==========================================
        _originalScale = bossTransform.localScale;
        _bossCollider = bossTransform.GetComponent<Collider2D>();
        _originalTag = bossTransform.tag;

        _renderers = bossTransform.GetComponentsInChildren<SpriteRenderer>(true);
        _originalSortingOrders = new int[_renderers.Length];
        for (int i = 0; i < _renderers.Length; i++) _originalSortingOrders[i] = _renderers[i].sortingOrder;

        // ==========================================
        // ACTO 0: LA ADVERTENCIA (Aún recibe daño)
        // ==========================================
        GameObject warningInstance = null;
        if (warningPrefab != null)
        {
            warningInstance = Instantiate(warningPrefab, bossTransform.position, Quaternion.identity, bossTransform);
        }

        float warnTimer = 0f;
        Vector3 basePos = bossTransform.localPosition;

        while (warnTimer < warningDuration)
        {
            warnTimer += Time.deltaTime;

            if (shakeBeforeEscape)
            {
                Vector2 randomShake = Random.insideUnitCircle * shakeIntensity;
                bossTransform.localPosition = basePos + (Vector3)randomShake;
            }
            yield return null;
        }

        if (warningInstance != null) Destroy(warningInstance);
        bossTransform.localPosition = basePos;

        // ==========================================
        // PREPARACIÓN DE INVULNERABILIDAD
        // ==========================================
        if (_bossCollider != null) _bossCollider.enabled = false;
        bossTransform.tag = "Untagged";

        // ==========================================
        // ACTO 1: EL ESCAPE
        // ==========================================
        Vector3 escapePos = new Vector3(zoneCenter.x, offScreenBottomY, bossTransform.localPosition.z);
        bossTransform.localRotation = Quaternion.Euler(0, 0, 180f);

        while (bossTransform.localPosition.y > offScreenBottomY + 0.5f)
        {
            bossTransform.localPosition = Vector3.MoveTowards(bossTransform.localPosition, escapePos, escapeSpeed * Time.deltaTime);
            yield return null;
        }

        // ==========================================
        // EL TRUCO: CAMBIO FUERA DE CÁMARA
        // ==========================================
        SetSortingOrders(backgroundSortingOrder);
        bossTransform.localScale = _originalScale * targetScale;
        bossTransform.localRotation = Quaternion.Euler(0, 0, 0f);
        BackgroundSpawner.Instance?.SetGlobalSpeedMultiplier(backgroundParallaxSpeed);

        // ← ACTO 2 COMIENZA: el boss ya es pequeño y está en el fondo
        DepthPhaseSignal.Enter();

        // ==========================================
        // ACTO 2: EL ASCENSO EN EL FONDO
        // ==========================================
        float currentY = bossTransform.localPosition.y;
        float timer = 0f;

        while (currentY < offScreenTopY)
        {
            currentY += ascendSpeed * Time.deltaTime;
            timer += Time.deltaTime;

            float offsetX = Mathf.Sin(timer * zigzagSpeed) * zigzagAmplitude;
            bossTransform.localPosition = new Vector3(zoneCenter.x + offsetX, currentY, bossTransform.localPosition.z);
            yield return null;
        }

        // ← ACTO 2 TERMINA: el boss ya salió del fondo, cortar el ataque antes de volver
        DepthPhaseSignal.Exit();

        // ==========================================
        // EL TRUCO PARTE 2: RESTAURACIÓN FUERA DE CÁMARA
        // ==========================================
        RestoreSortingOrders();
        bossTransform.localScale = _originalScale;
        bossTransform.localRotation = Quaternion.Euler(0, 0, 180f);
        bossTransform.localPosition = new Vector3(zoneCenter.x, offScreenTopY, bossTransform.localPosition.z);
        BackgroundSpawner.Instance?.SetGlobalSpeedMultiplier(1f);

        // ==========================================
        // ACTO 3: EL REGRESO TRIUNFAL
        // ==========================================
        Vector3 finalPos = new Vector3(zoneCenter.x, zoneCenter.y, bossTransform.localPosition.z);

        while (Vector3.Distance(bossTransform.localPosition, finalPos) > 0.1f)
        {
            bossTransform.localPosition = Vector3.MoveTowards(bossTransform.localPosition, finalPos, diveSpeed * Time.deltaTime);
            yield return null;
        }

        bossTransform.localPosition = finalPos;

        // ==========================================
        // FIN: RESTAURAR VULNERABILIDAD
        // ==========================================
        if (_bossCollider != null) _bossCollider.enabled = true;
        if (!string.IsNullOrEmpty(_originalTag)) bossTransform.tag = _originalTag;

        while (true) yield return null;
    }

    public override void OnStopped(Transform bossTransform)
    {
        // Seguridad: si el movimiento se interrumpe a mitad del ascenso, apagar el ataque
        DepthPhaseSignal.Exit();

        RestoreSortingOrders();
        BackgroundSpawner.Instance?.SetGlobalSpeedMultiplier(1f);

        if (_originalScale != Vector3.zero) bossTransform.localScale = _originalScale;
        bossTransform.localRotation = Quaternion.Euler(0, 0, 180f);

        if (_bossCollider != null) _bossCollider.enabled = true;
        if (!string.IsNullOrEmpty(_originalTag)) bossTransform.tag = _originalTag;

        _renderers = null;
        _originalSortingOrders = null;
        _bossCollider = null;
    }

    private void SetSortingOrders(int order)
    {
        if (_renderers == null) return;
        foreach (var sr in _renderers) if (sr != null) sr.sortingOrder = order;
    }

    private void RestoreSortingOrders()
    {
        if (_renderers == null || _originalSortingOrders == null) return;
        for (int i = 0; i < _renderers.Length && i < _originalSortingOrders.Length; i++)
            if (_renderers[i] != null) _renderers[i].sortingOrder = _originalSortingOrders[i];
    }
}