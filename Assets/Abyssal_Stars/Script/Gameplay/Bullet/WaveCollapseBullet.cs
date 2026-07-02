using UnityEngine;
using System;

public class WaveCollapseBullet : EnemyBullet
{
    private enum State { Traveling, OnRod, Collapsing }
    private State _state = State.Traveling;
    private bool _finishedNormally = false;

    private Vector2 _initialPosition;
    private Vector2 _moveDirection;  
    private float _speed;
    private float _timeElapsed;
    private float _distanceToAnchor;
    private float _amplitude;
    private float _frequency;

    private float[] _rodAngles;
    private float _rodRadius;  
    private Vector2 _anchorPoint;

    private float _collapseSpeedMult;

    public int waveIndex;
    public Action<int> OnCollapseCompleteWithIndex;
    private Action<WaveCollapseBullet> _onArrived;

    public void SetCollapseParameters(
        float amp,
        float freq,
        Vector2 direction,
        float speed,
        Vector2 anchor,
        int waveIdx,
        float[] rodAnglesRef,     
        float rodRadius,     
        float collapseSpeedMult,
        Action<WaveCollapseBullet> onArrived)
    {
        _amplitude = amp;
        _frequency = freq;
        _moveDirection = direction.normalized;
        _speed = speed;
        _anchorPoint = anchor;
        waveIndex = waveIdx;
        _rodAngles = rodAnglesRef;
        _rodRadius = rodRadius;
        _collapseSpeedMult = collapseSpeedMult;
        _onArrived = onArrived;

        _initialPosition = transform.position;
        _distanceToAnchor = Vector2.Distance(_initialPosition, _anchorPoint);

        _state = State.Traveling;
        _timeElapsed = 0f;
        _finishedNormally = false;
    }

    protected override void Update()
    {
        switch (_state)
        {
            case State.Traveling: UpdateTraveling(); break;
            case State.OnRod: UpdateOnRod(); break;
            case State.Collapsing: UpdateCollapsing(); break;
        }
    }

    private void UpdateTraveling()
    {
        _timeElapsed += Time.deltaTime;

        Vector2 perp = new Vector2(-_moveDirection.y, _moveDirection.x);
        float sineOff = Mathf.Sin(_timeElapsed * _frequency) * _amplitude;

        transform.position = _initialPosition
                           + _moveDirection * (_speed * _timeElapsed)
                           + perp * sineOff;

        if (_speed * _timeElapsed >= _distanceToAnchor)
        {
            _state = State.OnRod;
            _onArrived?.Invoke(this);
        }
    }

    private void UpdateOnRod()
    {
        if (_rodAngles == null) return;

        float rad = _rodAngles[waveIndex] * Mathf.Deg2Rad;
        Vector2 rodDir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
        transform.position = _anchorPoint + rodDir * _rodRadius;
    }

    private void UpdateCollapsing()
    {
        transform.position = Vector2.MoveTowards(
            transform.position,
            _anchorPoint,
            _speed * _collapseSpeedMult * Time.deltaTime);

        if (Vector2.Distance(transform.position, _anchorPoint) < 0.1f)
        {
            _finishedNormally = true; 
            OnCollapseCompleteWithIndex?.Invoke(waveIndex);
            ReturnToPool();
        }
    }

    public void StartCollapsing() => _state = State.Collapsing;
    public override void ResetBullet()
    {
        if (!_finishedNormally)
        {
            if (_state == State.Traveling)
                _onArrived?.Invoke(this);

            OnCollapseCompleteWithIndex?.Invoke(waveIndex);
        }

        base.ResetBullet();
        OnCollapseCompleteWithIndex = null;
        _onArrived = null;
        _rodAngles = null;
        _finishedNormally = false;
    }


}