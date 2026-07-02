using UnityEngine;

public class MobileInputManager : MonoBehaviour
{
    public static MobileInputManager Instance { get; private set; }

    [Header("Joystick")]
    [SerializeField] private Joystick _joystick;

    [Header("Umbral de Foco")]
    [Tooltip("Si la magnitud del joystick es menor a este valor se activa el modo foco. 0.4 = 40% del recorrido.")]
    [SerializeField] private float _focusThreshold = 0.4f;

    [Header("Detección de Plataforma")]
    [Tooltip("Fuerza el modo mobile en el editor para testear sin Android.")]
    [SerializeField] private bool _forceMobileInEditor = false;

    public Vector2 MoveDirection => _joystick != null
        ? new Vector2(_joystick.Horizontal, _joystick.Vertical)
        : Vector2.zero;

    // True si el joystick se mueve poco — activa modo foco automaticamente
    public bool IsFocusHeld => MoveDirection.magnitude > 0.05f
                            && MoveDirection.magnitude < _focusThreshold;

    public bool IsMobileActive =>
        Application.platform == RuntimePlatform.Android
        || Application.platform == RuntimePlatform.IPhonePlayer
        || (Application.isEditor && _forceMobileInEditor);

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    private void Start()
    {
        // Oculta los controles touch si estamos en PC
        if (!IsMobileActive)
            gameObject.SetActive(false);
    }
}