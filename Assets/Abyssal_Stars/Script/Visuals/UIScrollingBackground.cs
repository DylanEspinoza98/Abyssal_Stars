using UnityEngine;
using UnityEngine.UI;

public class UIBackgroundScroll : MonoBehaviour
{
    [Header("Ajustes de Velocidad")]
    [SerializeField] private float _scrollSpeed = 0.5f;

    private RawImage _bgRawImage;
    private Rect _uvRect;

    void Start()
    {
        _bgRawImage = GetComponent<RawImage>();
        _uvRect = _bgRawImage.uvRect;
    }

    void Update()
    {
        _uvRect.y += _scrollSpeed * Time.deltaTime;

        _bgRawImage.uvRect = _uvRect;
    }
}