using UnityEngine;

public class BombSmokeEffect : MonoBehaviour
{
    [Header("Configuración de Expansión")]
    [Tooltip("Qué tan rápido crecerá el humo en la pantalla")]
    [SerializeField] private float _expansionSpeed = 25f;

    [Tooltip("Tiempo en segundos antes de que el objeto se elimine por completo")]
    [SerializeField] private float _lifeTime = 1.5f;

    void Start()
    {
        Destroy(gameObject, _lifeTime);
    }

    void Update()
    {
        transform.localScale += Vector3.one * _expansionSpeed * Time.deltaTime;
    }
}