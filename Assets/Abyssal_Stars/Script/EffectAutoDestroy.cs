using UnityEngine;

public class EffectAutoDestroy : MonoBehaviour
{
    // Tiempo que tarda la animación en segundos
    // Puedes medirlo en la ventana de Animation
    [SerializeField] private float _lifeTime = 0.5f;

    void Start()
    {
        Destroy(gameObject, _lifeTime);
    }
}