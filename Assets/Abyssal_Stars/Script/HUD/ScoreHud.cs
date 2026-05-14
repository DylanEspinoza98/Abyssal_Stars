using UnityEngine;
using TMPro;

public class ScoreHUD : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _scoreText;

    private void Start()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged += UpdateScoreDisplay;

            UpdateScoreDisplay(ScoreManager.Instance.CurrentScore);
        }
    }

    private void OnDestroy()
    {

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged -= UpdateScoreDisplay;
        }
    }

    private void UpdateScoreDisplay(int newScore)
    {
        if (_scoreText != null)
        {
            _scoreText.text = $"Puntaje: {newScore:D6}";
        }
    }
}