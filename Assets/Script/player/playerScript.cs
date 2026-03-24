using UnityEngine;
using UnityEngine.InputSystem;

public class playerScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float movSpeed = 5f;
    float speedX, speedY;
    Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // 2. Nueva forma de leer el teclado/mando directamente
        // 'Keyboard.current' detecta si hay un teclado conectado
        Vector2 moveInput = Vector2.zero;

        if (Keyboard.current != null)
        {
            // Leemos las flechas o WASD (depende de cómo lo configures, 
            // pero esto es lo más directo para empezar)
            if (Keyboard.current.wKey.isPressed) moveInput.y = 1;
            if (Keyboard.current.sKey.isPressed) moveInput.y = -1;
            if (Keyboard.current.aKey.isPressed) moveInput.x = -1;
            if (Keyboard.current.dKey.isPressed) moveInput.x = 1;
        }

        speedX = moveInput.x * movSpeed;
        speedY = moveInput.y * movSpeed;

        rb.linearVelocity = new Vector2(speedX, speedY);
    }
}
