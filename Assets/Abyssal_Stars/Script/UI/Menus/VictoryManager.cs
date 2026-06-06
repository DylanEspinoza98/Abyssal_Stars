using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class VictoryManager : MonoBehaviour
{
    public static VictoryManager Instance { get; private set; }

    [Header("UI General")]
    [SerializeField] private GameObject _victoryPanel;
    [SerializeField] private TextMeshProUGUI _victoryText;
    [SerializeField] private Button _nextLevelButton;
    [SerializeField] private TextMeshProUGUI _nextLevelText;
    [SerializeField] private Button _menuButton;

    [Header("UI Rangos y Puntaje")]
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private TextMeshProUGUI _rankText;
    [SerializeField] private TextMeshProUGUI _bonusText;

    [Header("Configuración de Nivel")]
    [SerializeField] private bool _isLastLevel = false;
    [SerializeField] private string _nextLevelSceneName = "";
    [SerializeField] private string _menuSceneName = "MainMenu";

    [Header("Sistema de Rangos")]
    [SerializeField] private int _noBombBonus = 50000;
    [SerializeField] private int _scoreForRankS = 150000;
    [SerializeField] private int _scoreForRankA = 100000;
    [SerializeField] private int _scoreForRankB = 50000;

    [Header("Tiempos de Animación (Segundos)")]
    [SerializeField] private float _stepDelay = 0.5f;
    [SerializeField] private float _countDuration = 1.0f;

    [Header("Efectos de Sonido")]
    [SerializeField] private AudioClip _scoreTickingSound; // Sonido de conteo (loop)
    [SerializeField] private AudioClip _bonusSuccessSound; // Sonido al ganar el bono
    [SerializeField] private AudioClip _bonusFailSound;    // Sonido de error al perderlo
    [SerializeField] private AudioClip _rankRevealSound;   // El impacto del rango final

    private AudioSource _tickingAudioSource;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        _tickingAudioSource = gameObject.AddComponent<AudioSource>();
        _tickingAudioSource.loop = true;
        _tickingAudioSource.playOnAwake = false;
    }

    void Start()
    {
        _victoryPanel.SetActive(false);

        if (_isLastLevel)
        {
            if (_victoryText != null) _victoryText.text = "¡Gracias por Jugar!";
        }
        else
        {
            if (_victoryText != null) _victoryText.text = "¡Nivel Completado!";
            if (_nextLevelText != null) _nextLevelText.text = "Siguiente Nivel";
        }

        if (_nextLevelButton != null) _nextLevelButton.onClick.AddListener(GoToNextLevel);

        if (_menuButton != null)
        {
            TextMeshProUGUI menuText = _menuButton.GetComponentInChildren<TextMeshProUGUI>();
            if (menuText != null) menuText.text = "Menú Principal";
            _menuButton.onClick.AddListener(GoToMenu);
        }
    }

    public void ShowVictory()
    {
        if (ScoreHUD.Instance != null) ScoreHUD.Instance.HideHUD();
        _victoryPanel.SetActive(true);
        Time.timeScale = 0f;

        AudioSource audio = FindAnyObjectByType<AudioSource>();
        if (audio != null) audio.Stop();

        StartCoroutine(VictorySequenceRoutine());
    }

    private IEnumerator VictorySequenceRoutine()
    {
        if (_scoreText != null) _scoreText.text = "";
        if (_bonusText != null) _bonusText.text = "";
        if (_rankText != null) _rankText.text = "";

        if (_nextLevelButton != null) _nextLevelButton.gameObject.SetActive(false);
        if (_menuButton != null) _menuButton.gameObject.SetActive(false);

        int baseScore = ScoreManager.Instance != null ? ScoreManager.Instance.CurrentScore : 0;
        bool usedBomb = PlayerBomb.Instance != null && PlayerBomb.Instance.HasUsedBomb;
        int finalScore = baseScore + (!usedBomb ? _noBombBonus : 0);

        string finalRank = "C";
        Color rankColor = Color.gray;

        if (finalScore >= _scoreForRankS) finalRank = "S";
        else if (finalScore >= _scoreForRankA) finalRank = "A";
        else if (finalScore >= _scoreForRankB) finalRank = "B";

        if (!usedBomb && finalRank == "S") finalRank = "S+";
        else if (usedBomb && finalRank == "S") finalRank = "A";

        if (ScoreboardManager.Instance != null)
        {
            string currentSceneName = SceneManager.GetActiveScene().name;
            ScoreboardManager.Instance.AddScore(currentSceneName, finalScore, finalRank);
        }

        switch (finalRank)
        {
            case "S+": rankColor = new Color(0f, 1f, 1f); break;
            case "S": rankColor = new Color(1f, 0.84f, 0f); break;
            case "A": rankColor = Color.green; break;
            case "B": rankColor = new Color(1f, 0.5f, 0f); break;
            default: rankColor = Color.gray; break;
        }

        yield return new WaitForSecondsRealtime(_stepDelay);

        if (_scoreText != null)
        {
            if (_scoreTickingSound != null && _tickingAudioSource != null)
            {
                _tickingAudioSource.clip = _scoreTickingSound;
                _tickingAudioSource.Play();
            }

            float elapsedTime = 0f;
            while (elapsedTime < _countDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float lerpScore = Mathf.Lerp(0, baseScore, elapsedTime / _countDuration);
                _scoreText.text = $"Puntaje: {Mathf.RoundToInt(lerpScore)}";
                yield return null;
            }
            _scoreText.text = $"Puntaje: {baseScore}";

            if (_tickingAudioSource != null) _tickingAudioSource.Stop();
        }

        yield return new WaitForSecondsRealtime(_stepDelay);

        if (_bonusText != null)
        {
            if (!usedBomb)
            {
                if (_bonusSuccessSound != null)
                    AudioSource.PlayClipAtPoint(_bonusSuccessSound, Camera.main.transform.position);

                _bonusText.text = $"¡BONO NO-BOMB: +{_noBombBonus}!";
                _bonusText.color = Color.yellow;

                yield return new WaitForSecondsRealtime(_stepDelay);

                if (_scoreText != null)
                {
                    if (_scoreTickingSound != null && _tickingAudioSource != null)
                        _tickingAudioSource.Play();

                    float elapsedTime = 0f;
                    while (elapsedTime < _countDuration)
                    {
                        elapsedTime += Time.unscaledDeltaTime;
                        float lerpScore = Mathf.Lerp(baseScore, finalScore, elapsedTime / _countDuration);
                        _scoreText.text = $"Puntaje: {Mathf.RoundToInt(lerpScore)}";
                        yield return null;
                    }
                    _scoreText.text = $"Puntaje: {finalScore}";

                    if (_tickingAudioSource != null) _tickingAudioSource.Stop();
                }
            }
            else
            {
                if (_bonusFailSound != null)
                    AudioSource.PlayClipAtPoint(_bonusFailSound, Camera.main.transform.position);

                _bonusText.text = "Bono No-Bomb: Fallido (Castigo)";
                _bonusText.color = Color.red;
            }
        }

        yield return new WaitForSecondsRealtime(_stepDelay);

        if (_rankText != null)
        {
            _rankText.text = finalRank;

            if (_rankRevealSound != null)
            {
                AudioSource.PlayClipAtPoint(_rankRevealSound, Camera.main.transform.position);
            }

            float stampDuration = 0.3f; 
            float elapsedTime = 0f;

            Vector3 startScale = new Vector3(4f, 4f, 4f);
            Vector3 endScale = Vector3.one;

            Color transparentColor = rankColor;
            transparentColor.a = 0f;

            while (elapsedTime < stampDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float t = elapsedTime / stampDuration;
                float easeIn = t * t;

                _rankText.rectTransform.localScale = Vector3.Lerp(startScale, endScale, easeIn);
                _rankText.color = Color.Lerp(transparentColor, rankColor, easeIn);

                yield return null;
            }

            _rankText.rectTransform.localScale = endScale;
            _rankText.color = rankColor;
        }

        yield return new WaitForSecondsRealtime(_stepDelay);

        if (!_isLastLevel && _nextLevelButton != null) _nextLevelButton.gameObject.SetActive(true);
        if (_menuButton != null) _menuButton.gameObject.SetActive(true);
    }

    private void GoToNextLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(_nextLevelSceneName);
    }

    private void GoToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(_menuSceneName);
    }
}