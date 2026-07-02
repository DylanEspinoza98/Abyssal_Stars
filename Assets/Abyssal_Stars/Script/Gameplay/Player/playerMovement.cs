using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerHealth), typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float movSpeed = 5f;
    [SerializeField] private float _focusSpeedMultiplier = 0.4f;
    [SerializeField] private float _limitX = 4.5f;
    [SerializeField] private float _limitY = 8.5f;

    [Header("Visuals")]
    [SerializeField] private Animator _thrusterAnimator;

    private Rigidbody2D rb;
    private PlayerHealth health;
    private MobileInputManager _mobileInput;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<PlayerHealth>();
        _mobileInput = MobileInputManager.Instance;
    }

    void Update()
    {
        if (Time.timeScale == 0f) return;

        if (health != null && health.IsDead)
        {
            if (_thrusterAnimator != null) _thrusterAnimator.SetBool("isMoving", false);
            return;
        }

        HandleMovement();
    }

    private void HandleMovement()
    {
        Vector2 moveInput = Vector2.zero;
        float currentSpeed = movSpeed;

        bool isMobile = _mobileInput != null && _mobileInput.IsMobileActive;

        if (isMobile)
        {
            moveInput = _mobileInput.MoveDirection;

            // Si el joystick se mueve poco activa el modo foco automaticamente
            if (_mobileInput.IsFocusHeld)
                currentSpeed *= _focusSpeedMultiplier;
        }
        else if (Keyboard.current != null && DataManager.Instance != null)
        {
            SettingsData settings = DataManager.Instance.SaveData.settings;

            if (IsKeyPressed(settings.focusKey)) currentSpeed *= _focusSpeedMultiplier;

            if (IsKeyPressed(settings.moveUpKey)) moveInput.y = 1;
            if (IsKeyPressed(settings.moveDownKey)) moveInput.y = -1;
            if (IsKeyPressed(settings.moveLeftKey)) moveInput.x = -1;
            if (IsKeyPressed(settings.moveRightKey)) moveInput.x = 1;
        }

        rb.linearVelocity = moveInput.normalized * currentSpeed;

        bool isMoving = moveInput != Vector2.zero;
        if (_thrusterAnimator != null)
            _thrusterAnimator.SetBool("isMoving", isMoving);

        float clampedX = Mathf.Clamp(transform.localPosition.x, -_limitX, _limitX);
        float clampedY = Mathf.Clamp(transform.localPosition.y, -_limitY, _limitY);
        transform.localPosition = new Vector3(clampedX, clampedY, transform.localPosition.z);
    }

    private bool IsKeyPressed(string keyName)
    {
        if (string.IsNullOrEmpty(keyName) || Keyboard.current == null) return false;

        foreach (var key in Keyboard.current.allKeys)
        {
            if (key.name.Equals(keyName, System.StringComparison.OrdinalIgnoreCase))
                return key.isPressed;
        }
        return false;
    }
}