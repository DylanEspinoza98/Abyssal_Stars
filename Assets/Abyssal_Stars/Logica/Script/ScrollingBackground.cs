using UnityEngine;

public class ScrollingBackground : MonoBehaviour
{
    private Transform _cam;

    void Start()
    {
        _cam = Camera.main.transform;
    }

    void Update()
    {
        // Sigue la cámara en Y, se queda fijo en X y Z
        transform.position = new Vector3(
            transform.position.x,
            _cam.position.y,
            transform.position.z
        );
    }
}