using UnityEngine;

// Se encarga exclusivamente de iniciar la secuencia de victoria
// cuando el jefe muere. BossController no sabe nada de este script.
public class LevelWinHandler : MonoBehaviour
{
    private void OnEnable()
    {
        BossController.OnBossDefeated += HandleBossDefeated;
    }

    private void OnDisable()
    {
        BossController.OnBossDefeated -= HandleBossDefeated;
    }

    private void HandleBossDefeated()
    {
        VictoryManager.Instance?.ShowVictory();
    }
}