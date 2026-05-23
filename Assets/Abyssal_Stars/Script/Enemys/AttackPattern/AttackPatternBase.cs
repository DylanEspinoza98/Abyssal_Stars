using UnityEngine;
using System.Collections;

public abstract class AttackPatternSO : ScriptableObject
{
    [Header("Dirección")]
    [Tooltip("Invierte la dirección horizontal del patrón. " +
             "Útil para que la torreta izquierda y derecha sean simétricas " +
             "usando el mismo SO con distinta configuración.")]
    public bool mirrorX = false;

    protected float ApplyMirror(float angle)
    {
        return mirrorX ? 180f - angle : angle;
    }

    public abstract IEnumerator ExecutePattern(BossTurret turret);
}