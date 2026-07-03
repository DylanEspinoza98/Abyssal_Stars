using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class MobileInputManager : MonoBehaviour
{
    public static MobileInputManager Instance { get; private set; }

    [Header("Touch Drag (estilo Touhou movil)")]
    [SerializeField] private Camera _camera;
    [Tooltip("1 = el player sigue al dedo 1:1 en el mundo. >1 amplifica el movimiento para no tener que recorrer toda la pantalla.")]
    [SerializeField] private float _sensitivity = 1.2f;
    [Tooltip("Ignora toques que empiezan sobre elementos de UI (ej. boton de pausa).")]
    [SerializeField] private bool _ignoreUITouches = true;

    [Header("Deteccion de Plataforma")]
    [Tooltip("Fuerza el modo mobile en el editor para testear con el mouse.")]
    [SerializeField] private bool _forceMobileInEditor = false;

    public bool IsMobileActive =>
        Application.platform == RuntimePlatform.Android
        || Application.platform == RuntimePlatform.IPhonePlayer
        || (Application.isEditor && _forceMobileInEditor);

    // True mientras el dedo se esta arrastrando por la pantalla (para el thruster, etc.)
    public bool HasDrag { get; private set; }

    private Vector2 _accumulatedDelta;
    private Vector3? _lastWorldPos;
    private bool _touchStartedOverUI;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    private void Start()
    {
        if (_camera == null) _camera = Camera.main;

        // En PC no leemos touch; se apaga el contenedor de controles tactiles.
        if (!IsMobileActive)
            gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!IsMobileActive) return;
        ReadDrag();
    }

    private void ReadDrag()
    {
        // Si el juego esta pausado no acumulamos movimiento (evita saltos al reanudar).
        if (Time.timeScale == 0f)
        {
            _lastWorldPos = null;
            HasDrag = false;
            return;
        }

        bool pressed = false;
        Vector2 screenPos = Vector2.zero;
        int fingerId = -1;

        var ts = Touchscreen.current;
        if (ts != null && ts.primaryTouch.press.isPressed)
        {
            pressed = true;
            screenPos = ts.primaryTouch.position.ReadValue();
            fingerId = ts.primaryTouch.touchId.ReadValue();
        }
        else if (Application.isEditor && Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            // Fallback para testear en el editor con el mouse.
            pressed = true;
            screenPos = Mouse.current.position.ReadValue();
        }

        if (!pressed)
        {
            _lastWorldPos = null;
            HasDrag = false;
            return;
        }

        // Primer frame del toque: solo registramos el punto de partida, sin mover.
        if (_lastWorldPos == null)
        {
            _touchStartedOverUI = _ignoreUITouches
                && EventSystem.current != null
                && EventSystem.current.IsPointerOverGameObject(fingerId);
            _lastWorldPos = _camera.ScreenToWorldPoint(screenPos);
            HasDrag = false;
            return;
        }

        // Si el toque empezo sobre un boton de UI, no movemos al player.
        if (_touchStartedOverUI) return;

        Vector3 currentWorld = _camera.ScreenToWorldPoint(screenPos);
        Vector2 delta = (Vector2)(currentWorld - _lastWorldPos.Value) * _sensitivity;

        _accumulatedDelta += delta;
        _lastWorldPos = currentWorld;
        HasDrag = delta.sqrMagnitude > 0.0000001f;
    }

    // Devuelve el movimiento acumulado (en unidades de mundo) desde la ultima llamada y lo resetea.
    // PlayerMovement lo consume en FixedUpdate.
    public Vector2 ConsumeDragDelta()
    {
        Vector2 d = _accumulatedDelta;
        _accumulatedDelta = Vector2.zero;
        return d;
    }
}
