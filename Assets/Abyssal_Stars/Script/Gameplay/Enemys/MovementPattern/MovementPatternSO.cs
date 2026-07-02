using UnityEngine;
using System.Collections;

public abstract class MovementPatternSO : ScriptableObject
{
    public abstract IEnumerator ExecuteMovement(Transform bossTransform, Vector2 zoneCenter);
    public virtual void OnStopped(Transform bossTransform) { }
}