using UnityEngine;

public class SpawnerPatrol : MonoBehaviour
{
    [Header("Ajustes de Patrulla")]
    [SerializeField] private float _rangeX = 4f;
    [SerializeField] private float _speed = 3f;

    private float _startX;

    void Start()
    {
        _startX = transform.localPosition.x;
    }

    void Update()
    {
        float offset = Mathf.Sin(Time.time * _speed) * _rangeX;

        float newX = _startX + offset;

        transform.localPosition = new Vector3(newX, transform.localPosition.y, transform.localPosition.z);
    }
}