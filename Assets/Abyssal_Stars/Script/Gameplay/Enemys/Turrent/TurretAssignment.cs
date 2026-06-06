using UnityEngine;

[System.Serializable]
public class TurretAssignment
{
    [Tooltip("Torreta hija del boss.")]
    public BossTurret turret;

    [Tooltip("Si está desactivada en una fase, se oculta y deja de disparar.")]
    public bool activeByDefault = true;
}