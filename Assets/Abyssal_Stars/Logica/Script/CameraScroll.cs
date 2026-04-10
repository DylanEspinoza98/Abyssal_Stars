using UnityEngine;

public class CameraScroll : MonoBehaviour
{
    [SerializeField] private float _scrollSpeed = 2f;

    void Update()
    {
        // La cámara sube constantemente en el eje Y
        transform.Translate(Vector2.up * _scrollSpeed * Time.deltaTime);
    }
}