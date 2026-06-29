using UnityEngine;

[RequireComponent(typeof(PlayerHealth), typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float _movSpeed = 5f;
    [SerializeField] private float _focusSpeedMultiplier = 0.4f;
    [SerializeField] private float _limitX = 4.5f;
    [SerializeField] private float _limitY = 8.5f;

    [Header("Visuals")]
    [SerializeField] private Animator _thrusterAnimator;

    private Rigidbody2D rb;
    private PlayerHealth health;
    private Vector2 _moveInput;
    private float _currentSpeed;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<PlayerHealth>();
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

        ReadInput();
    }

    private void FixedUpdate()
    {
        ApplyMovement();
    }

    private void ReadInput()
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

    private void ApplyMovement()
    {
        rb.linearVelocity = _moveInput.normalized * _currentSpeed;

        float clampedX = Mathf.Clamp(transform.localPosition.x, -_limitX, _limitX);
        float clampedY = Mathf.Clamp(transform.localPosition.y, -_limitY, _limitY);
        transform.localPosition = new Vector3(clampedX, clampedY, transform.localPosition.z);
    }
}