using UnityEngine;
using TMPro;

public class ScoreHUD : MonoBehaviour
{
    public static ScoreHUD Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI _scoreText;

    private bool _isBlackHoleActive = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged += UpdateScoreDisplay;
            ScoreManager.Instance.OnMultiplierChanged += ForceMultiplierUpdate;
            ScoreManager.Instance.OnBlackHoleToggled += HandleBlackHole;

            UpdateScoreDisplay(ScoreManager.Instance.CurrentScore);
        }
    }

    private void OnDestroy()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged -= UpdateScoreDisplay;
            ScoreManager.Instance.OnMultiplierChanged -= ForceMultiplierUpdate;
            ScoreManager.Instance.OnBlackHoleToggled -= HandleBlackHole;
        }
    }

    private void ForceMultiplierUpdate(float newMultiplier)
    {
        if (ScoreManager.Instance != null)
        {
            UpdateScoreDisplay(ScoreManager.Instance.CurrentScore);
        }
    }

    private void HandleBlackHole(bool isActive)
    {
        _isBlackHoleActive = isActive;
        if (ScoreManager.Instance != null)
        {
            UpdateScoreDisplay(ScoreManager.Instance.CurrentScore);
        }
    }

    private void UpdateScoreDisplay(int newScore)
    {
        if (_scoreText == null) return;

        if (_isBlackHoleActive)
        {
            _scoreText.text = $"Puntaje: <color=#FF5555>{newScore:D6} (BLOQUEADO)</color>";
            return;
        }

        float mult = ScoreManager.Instance.CurrentMultiplier;

        if (mult > 1.0f)
        {
            _scoreText.text = $"Puntaje: {newScore:D6} <color=yellow>(x{mult})</color>";
        }
        else
        {
            _scoreText.text = $"Puntaje: {newScore:D6}";
        }
    }

    public void HideHUD()
    {
        gameObject.SetActive(false);
    }
}