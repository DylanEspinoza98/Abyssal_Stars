using UnityEngine;
using System;

public class SpaceDecor : MonoBehaviour
{
    public event Action<SpaceDecor> OnOutOfBounds;

    private float _speed;
    private float _killY;
    private bool _triggered;

    private float _aliveTime;
    private const float MIN_LIFETIME = 1.0f;

    public void Setup(float speed, float killY)
    {
        _speed = speed;
        _killY = killY;
        _aliveTime = 0f;
        _triggered = false;
    }

    private void Update()
    {
        if (_triggered) return;

        // 1. Movimiento puro hacia abajo
        transform.Translate(Vector3.down * _speed * Time.deltaTime, Space.World);

        // 2. Temporizador rápido
        _aliveTime += Time.deltaTime;

        // 3. Única regla de muerte: Cruzar la coordenada 
        if (_aliveTime >= MIN_LIFETIME && transform.position.y <= _killY)
        {
            _triggered = true;
            OnOutOfBounds?.Invoke(this);
        }
    }
}