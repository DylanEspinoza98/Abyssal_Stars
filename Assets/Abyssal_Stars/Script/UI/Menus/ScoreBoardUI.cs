using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class ScoreboardUI : MonoBehaviour
{
    [Header("Referencias de Textos")]
    [Tooltip("El texto que muestra el nombre del nivel actual")]
    [SerializeField] private TextMeshProUGUI _levelNameText;
    [Tooltip("El texto central donde se mostrar el Top 5")]
    [SerializeField] private TextMeshProUGUI _scoresText;

    [Header("Botones de Navegación")]
    [SerializeField] private Button _prevButton;
    [SerializeField] private Button _nextButton;

    private List<string> _playedLevels = new List<string>();
    private int _currentLevelIndex = 0;

    private void OnEnable()
    {
        LoadLevelData();
    }

    private void Start()
    {
        if (_prevButton != null) _prevButton.onClick.AddListener(ShowPreviousLevel);
        if (_nextButton != null) _nextButton.onClick.AddListener(ShowNextLevel);
    }

    private void LoadLevelData()
    {
        if (ScoreboardManager.Instance == null || DataManager.Instance == null) return;

        var allScores = DataManager.Instance.SaveData.scoreboard.levelScores;
        _playedLevels = allScores.Select(l => l.levelID).ToList();

        if (_playedLevels.Count == 0)
        {
            if (_levelNameText != null) _levelNameText.text = "SIN DATOS";

            if (_scoresText != null) _scoresText.text = "Aún no hay récords guardados.\n¡Ve y juega una partida!";

            if (_prevButton != null) _prevButton.interactable = false;
            if (_nextButton != null) _nextButton.interactable = false;
            return;
        }

        if (_prevButton != null) _prevButton.interactable = true;
        if (_nextButton != null) _nextButton.interactable = true;

        _currentLevelIndex = Mathf.Clamp(_currentLevelIndex, 0, _playedLevels.Count - 1);
        UpdateDisplay();
    }

    public void ShowNextLevel()
    {
        if (_playedLevels.Count == 0) return;

        _currentLevelIndex++;
        if (_currentLevelIndex >= _playedLevels.Count)
        {
            _currentLevelIndex = 0;
        }

        UpdateDisplay();
    }

    public void ShowPreviousLevel()
    {
        if (_playedLevels.Count == 0) return;

        _currentLevelIndex--;
        if (_currentLevelIndex < 0)
        {
            _currentLevelIndex = _playedLevels.Count - 1;
        }

        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (_playedLevels.Count == 0) return;

        string selectedLevel = _playedLevels[_currentLevelIndex];

        if (_levelNameText != null)
        {
            _levelNameText.text = selectedLevel.Replace("_", " ").ToUpper();
        }

        DisplayScores(selectedLevel);
    }

    private void DisplayScores(string levelID)
    {
        if (_scoresText == null) return;

        List<ScoreEntry> scores = ScoreboardManager.Instance.GetScoresForLevel(levelID);

        if (scores == null || scores.Count == 0)
        {
            _scoresText.text = "No hay puntajes para este nivel.";
            return;
        }

        string result = "";
        for (int i = 0; i < scores.Count; i++)
        {
            result += $"<color=yellow>{i + 1}.</color> Rango {scores[i].rank} <color=#AAAAAA>|</color> {scores[i].score:D6} pts\n\n";
        }

        _scoresText.text = result;
    }
}