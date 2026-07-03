using UnityEngine;

[RequireComponent(typeof(PlayerHealth), typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float _movSpeed = 5f;
    [SerializeField] private float _focusSpeedMultiplier = 0.4f;

    [Header("Bounds")]
    [SerializeField] private Camera _camera;                                // camara ortografica (dejar vacio para usar Camera.main)
    [SerializeField] private Vector2 _edgePadding = new Vector2(0.3f, 0.3f); // margen ~ medio tamano del sprite del player

    [Header("Visuals")]
    [SerializeField] private Animator _thrusterAnimator;

    private Rigidbody2D rb;
    private PlayerHealth health;
    private Vector2 _moveInput;
    private float _currentSpeed;
    private MobileInputManager _mobileInput;
    private bool _isMobile;

    private float _limitX;
    private float _limitY;
    private int _lastW;
    private int _lastH;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<PlayerHealth>();
        _mobileInput = MobileInputManager.Instance;

        if (_camera == null) _camera = Camera.main;
        RecalculateLimits();
    }

    // Calcula los limites del area de juego a partir de lo que la camara muestra,
    // asi se adaptan a cualquier resolucion / aspect ratio en vez de ser fijos.
    private void RecalculateLimits()
    {
        if (_camera == null) return;

        float halfHeight = _camera.orthographicSize;
        float halfWidth = halfHeight * _camera.aspect;

        _limitX = halfWidth - _edgePadding.x;
        _limitY = halfHeight - _edgePadding.y;

        _lastW = Screen.width;
        _lastH = Screen.height;
    }

    void Update()
    {
        if (Time.timeScale == 0f)
        {
            _moveInput = Vector2.zero;
            return;
        }

        if (health != null && health.IsDead)
        {
            _moveInput = Vector2.zero;
            if (_thrusterAnimator != null) _thrusterAnimator.SetBool("isMoving", false);
            return;
        }

        _isMobile = _mobileInput != null && _mobileInput.IsMobileActive;

        // El input tactil se lee/consume en FixedUpdate; en PC leemos aqui.
        if (!_isMobile)
            ReadInputPC();
    }

    private void FixedUpdate()
    {
        // recalcula si cambio la resolucion (rotacion de pantalla, ventana redimensionada, etc.)
        if (Screen.width != _lastW || Screen.height != _lastH)
            RecalculateLimits();

        if (_isMobile)
            ApplyTouchDrag();
        else
            ApplyMovementPC();
    }

    private void ReadInputPC()
    {
        _moveInput = Vector2.zero;
        _currentSpeed = _movSpeed;

        if (InputManager.Instance == null) return;

        if (InputManager.Instance.Focus.IsPressed())
            _currentSpeed *= _focusSpeedMultiplier;

        _moveInput = InputManager.Instance.Move.ReadValue<Vector2>();

        if (_thrusterAnimator != null)
            _thrusterAnimator.SetBool("isMoving", _moveInput != Vector2.zero);
    }

    private void ApplyMovementPC()
    {
        rb.linearVelocity = _moveInput.normalized * _currentSpeed;

        float clampedX = Mathf.Clamp(transform.localPosition.x, -_limitX, _limitX);
        float clampedY = Mathf.Clamp(transform.localPosition.y, -_limitY, _limitY);
        transform.localPosition = new Vector3(clampedX, clampedY, transform.localPosition.z);
    }

    // Movimiento por arrastre relativo (estilo Touhou movil): el player sigue el
    // delta del dedo. Como el delta se acumula por frame, la velocidad varia
    // automaticamente segun que tan rapido arrastres.
    private void ApplyTouchDrag()
    {
        rb.linearVelocity = Vector2.zero;

        Vector2 delta = _mobileInput.ConsumeDragDelta();

        // Objetivo con el delta aplicado, ya recortado a los limites de pantalla.
        Vector2 target = rb.position + delta;
        target.x = Mathf.Clamp(target.x, -_limitX, _limitX);
        target.y = Mathf.Clamp(target.y, -_limitY, _limitY);
        rb.MovePosition(target);

        if (_thrusterAnimator != null)
            _thrusterAnimator.SetBool("isMoving", delta.sqrMagnitude > 0.0000001f);
    }
}
