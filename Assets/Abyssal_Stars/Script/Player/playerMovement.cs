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

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        health = GetComponent<PlayerHealth>();
    }

    void Update()
    {
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

        if (Keyboard.current != null)
        {
            if (Keyboard.current.leftShiftKey.isPressed) currentSpeed *= _focusSpeedMultiplier;
            if (Keyboard.current.wKey.isPressed) moveInput.y = 1;
            if (Keyboard.current.sKey.isPressed) moveInput.y = -1;
            if (Keyboard.current.aKey.isPressed) moveInput.x = -1;
            if (Keyboard.current.dKey.isPressed) moveInput.x = 1;
        }

        rb.linearVelocity = moveInput.normalized * currentSpeed;

        bool isMoving = moveInput != Vector2.zero;

        if (_thrusterAnimator != null)
        {
            _thrusterAnimator.SetBool("isMoving", isMoving);
        }

        float clampedX = Mathf.Clamp(transform.localPosition.x, -_limitX, _limitX);
        float clampedY = Mathf.Clamp(transform.localPosition.y, -_limitY, _limitY);
        transform.localPosition = new Vector3(clampedX, clampedY, transform.localPosition.z);
    }
}