using UnityEngine;

public class SpawnerPatrol : MonoBehaviour
{
    [Header("Ajustes de Patrulla")]
    [SerializeField] private float _rangeX = 4f;  // Qué tanto se mueve a los lados
    [SerializeField] private float _speed = 3f;   // Qué tan rápido oscila


    void Update()
    {
        // Usamos la función Seno para crear un movimiento de vaivén (oscilación)
        float newX = Mathf.Sin(Time.time * _speed) * _rangeX;

        // Aplicamos la posición local (dentro de la cámara)
        transform.localPosition = new Vector3(newX, transform.localPosition.y, transform.localPosition.z);
    }
}