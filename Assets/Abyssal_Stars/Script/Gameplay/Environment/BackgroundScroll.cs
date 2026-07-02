using UnityEngine;
public class BackgroundScroll : MonoBehaviour

{
    [Header("Ajustes de Velocidad")]
    [SerializeField] private float _scrollSpeed = 0.5f;
    private Material _bgMaterial;

    private Vector2 _offset;
    void Start()

    {
        _bgMaterial = GetComponent<Renderer>().material;

    }



    void Update()

    {
        _offset.y += _scrollSpeed * Time.deltaTime;
        _bgMaterial.mainTextureOffset = _offset;
    }

}

