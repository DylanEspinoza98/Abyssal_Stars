using UnityEngine;

[CreateAssetMenu(fileName = "New Boss Phase", menuName = "Boss Patterns/Boss Phase")]
public class BossPhaseSO : ScriptableObject
{
    [Tooltip("Patrón de movimiento durante esta fase.")]
    public MovementPatternSO movementPattern;

    [Tooltip("Patrones de disparo por torreta — el orden debe coincidir " +
             "con el array _phaseSetups del BossController.")]
    public AttackPatternSO[] turretPatterns;

    [Tooltip("Duración de la fase en segundos.")]
    public float duration = 5f;

    [Tooltip("Pausa entre esta fase y la siguiente.")]
    public float transitionDelay = 1f;
}