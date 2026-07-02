using UnityEngine;
using System;

public class SpaceDecor : MonoBehaviour
{
    public event Action<SpaceDecor> OnOutOfBounds;

    private float _baseSpeed;
    private float _multiplier = 1f;
    private float _killY;
    private bool _triggered;

    private float _aliveTime;
    private const float MIN_LIFETIME = 1.0f;

    public void Setup(float speed, float killY)
    {
        _baseSpeed = speed;
        _multiplier = 1f;  
        _killY = killY;
        _aliveTime = 0f;
        _triggered = false;
    }

    public void SetMultiplier(float m) => _multiplier = m;

    private void Update()
    {
        if (_triggered) return;

        transform.Translate(Vector3.down * _baseSpeed * _multiplier * Time.deltaTime, Space.World);

        _aliveTime += Time.deltaTime;

        if (_aliveTime >= MIN_LIFETIME && transform.position.y <= _killY)
        {
            _triggered = true;
            OnOutOfBounds?.Invoke(this);
        }
    }
}