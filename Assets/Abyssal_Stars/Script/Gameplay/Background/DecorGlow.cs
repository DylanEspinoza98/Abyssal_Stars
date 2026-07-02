using UnityEngine;

/// <summary>
/// Efecto de glow pulsante sobre el SpriteRenderer del objeto.
/// No usa shaders externos: modula el color y escala del sprite.
/// Compatible con cualquier sprite sin URP/HDRP requerido.
///
/// Para un glow real con URP: activá "Bloom" en el Volume y asigná
/// un material Unlit/Additive al SpriteRenderer.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class DecorGlow : MonoBehaviour
{
    //  Configuración
    private float _baseAlpha;    
    private float _glowIntensity;    
    private float _glowSpeed;        
    private float _scaleBreath;      

    private SpriteRenderer _sr;
    private Vector3        _baseScale;
    private float          _timeOffset;   

    // API pública 

    /// <summary>
    /// Inicializa el glow. Llamar después de setear la escala del objeto.
    /// </summary>
    /// <param name="baseAlpha">Alfa base del sprite (0-1).</param>
    /// <param name="glowIntensity">Amplitud de la oscilación de alfa (recomendado 0.1-0.25).</param>
    /// <param name="glowSpeed">Frecuencia del pulso (recomendado 0.8-2.5).</param>
    /// <param name="scaleBreath">Amplitud de "respiración" de escala (0 = sin efecto).</param>
    public void Setup(float baseAlpha, float glowIntensity = 0.15f,
                      float glowSpeed = 1.2f,  float scaleBreath = 0.02f)
    {
        _sr            = GetComponent<SpriteRenderer>();
        _baseAlpha     = baseAlpha;
        _glowIntensity = glowIntensity;
        _glowSpeed     = glowSpeed;
        _scaleBreath   = scaleBreath;
        _baseScale     = transform.localScale;
        _timeOffset    = Random.Range(0f, Mathf.PI * 2f);   // fase aleatoria
    }

    // Unity 

    private void Update()
    {
        float sin = Mathf.Sin(Time.time * _glowSpeed + _timeOffset);

        // Pulso de alfa
        if (_sr != null)
        {
            Color c = _sr.color;
            c.a     = Mathf.Clamp01(_baseAlpha + sin * _glowIntensity);
            _sr.color = c;
        }

        // Respiración de escala (muy sutil, hace que parezca vivo)
        if (_scaleBreath > 0f)
        {
            float breathFactor = 1f + sin * _scaleBreath;
            transform.localScale = _baseScale * breathFactor;
        }
    }

    /// <summary>
    /// Resetea el estado cuando el objeto vuelve al pool.
    /// </summary>
    public void ResetGlow()
    {
        if (_sr != null)
        {
            Color c = _sr.color;
            c.a     = _baseAlpha;
            _sr.color = c;
        }

        transform.localScale = _baseScale;
        _timeOffset = Random.Range(0f, Mathf.PI * 2f);   
    }
}
