using UnityEngine;

[CreateAssetMenu(fileName = "New Boss Phase", menuName = "Boss Patterns/Boss Phase")]
public class BossPhaseSO : ScriptableObject
{
    [Tooltip("Patr�n de movimiento durante esta fase.")]
    public MovementPatternSO movementPattern;

    [Tooltip("Patrones de disparo por torreta � el orden debe coincidir " +
             "con el array _phaseSetups del BossController.")]
    public AttackPatternSO[] turretPatterns;

    [Tooltip("Duraci�n de la fase en segundos.")]
    [SerializeField] private float duration = 5f;
    public float Duration => duration;

    [Tooltip("Pausa entre esta fase y la siguiente.")]
    [SerializeField] private float transitionDelay = 1f;
    public float TransitionDelay => transitionDelay;
}