using UnityEngine;
using System.Collections;

public abstract class AttackPatternSO : ScriptableObject
{
    public abstract IEnumerator ExecutePattern(BossTurret turret);
}