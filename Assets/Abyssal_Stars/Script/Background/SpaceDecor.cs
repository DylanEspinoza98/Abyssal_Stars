using UnityEngine;

public class SpaceDecor : MonoBehaviour
{
    [Header("Límites de Pantalla")]
    [SerializeField] private float _offScreenMargin = 0.2f;

    [Header("Seguridad Inicial (Temporizador)")]
    [SerializeField] private float _checkDelay = 2.5f;

    private float _timer = 0f;
    private Camera _mainCam;

    private void Start()
    {
        _mainCam = Camera.main;
    }

    private void Update()
    {
        _timer += Time.deltaTime;

        if (_timer >= _checkDelay)
        {
            if (IsOutOfScreen())
            {
                Destroy(gameObject);
            }
        }
    }

    private bool IsOutOfScreen()
    {
        if (_mainCam == null) return false;

        Vector2 vp = _mainCam.WorldToViewportPoint(transform.position);

        return vp.x < -_offScreenMargin || vp.x > 1 + _offScreenMargin ||
               vp.y < -_offScreenMargin || vp.y > 1 + _offScreenMargin;
    }
}