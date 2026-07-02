using UnityEngine;

/// <summary>
/// Gestor de input tactil para plataformas moviles (Android / iOS).
/// Vive en la escena de gameplay junto al Canvas de controles tactiles y
/// alimenta al InputManager central cuando la plataforma es movil.
///
/// Configuracion en escena:
///   1. Crear un Canvas de controles (Screen Space - Overlay).
///   2. Arrastrar un prefab de joystick del "Joystick Pack" (p.ej. Fixed Joystick)
///      dentro del Canvas.
///   3. Anadir un GameObject vacio con este componente y asignarle el joystick.
///   4. (Opcional) Botones de bomba/pausa que llamen a PressBomb() / PressPause().
/// </summary>
public class MobileInputManager : MonoBehaviour
{
    public static MobileInputManager Instance { get; private set; }

    [Header("Joystick")]
    [SerializeField] private Joystick _joystick;

    [Header("Umbral de Foco")]
    [Tooltip("Si la magnitud del joystick es menor a este valor se activa el modo foco. 0.4 = 40% del recorrido.")]
    [SerializeField] private float _focusThreshold = 0.4f;

    [Header("Disparo")]
    [Tooltip("En movil el disparo es automatico mientras esta activo.")]
    [SerializeField] private bool _autoFire = true;

    [Header("Deteccion de Plataforma")]
    [Tooltip("Fuerza el modo movil en el editor para testear sin Android.")]
    [SerializeField] private bool _forceMobileInEditor = false;

    // ── Estado de botones (se consumen una sola vez) ───────────────────────
    private bool _bombPressed;
    private bool _pausePressed;

    // ── API de lectura ─────────────────────────────────────────────────────
    /// <summary>Direccion de movimiento del joystick (Vector2, -1..1 por eje).</summary>
    public Vector2 MoveDirection => _joystick != null
        ? new Vector2(_joystick.Horizontal, _joystick.Vertical)
        : Vector2.zero;

    /// <summary>True si el joystick se mueve poco: activa el modo foco automaticamente.</summary>
    public bool IsFocusHeld => MoveDirection.magnitude > 0.05f
                            && MoveDirection.magnitude < _focusThreshold;

    /// <summary>En movil el disparo es automatico (autofire).</summary>
    public bool ShootHeld => _autoFire;

    /// <summary>True si la plataforma actual usa controles tactiles.</summary>
    public bool IsMobileActive =>
        Application.platform == RuntimePlatform.Android
        || Application.platform == RuntimePlatform.IPhonePlayer
        || (Application.isEditor && _forceMobileInEditor);

    // ── Botones opcionales (conectar desde eventos OnClick de la UI) ───────
    /// <summary>Llamar desde el boton de bomba (UI Button > OnClick).</summary>
    public void PressBomb() => _bombPressed = true;

    /// <summary>Llamar desde el boton de pausa (UI Button > OnClick).</summary>
    public void PressPause() => _pausePressed = true;

    /// <summary>Devuelve true una sola vez tras pulsar la bomba.</summary>
    public bool ConsumeBombPress()
    {
        if (!_bombPressed) return false;
        _bombPressed = false;
        return true;
    }

    /// <summary>Devuelve true una sola vez tras pulsar la pausa.</summary>
    public bool ConsumePausePress()
    {
        if (!_pausePressed) return false;
        _pausePressed = false;
        return true;
    }

    // ── Lifecycle ──────────────────────────────────────────────────────────
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    private void Start()
    {
        // Oculta los controles tactiles si no estamos en movil (PC).
        if (!IsMobileActive)
            gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}
