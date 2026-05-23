using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance { get; private set; }

    [SerializeField] private GameObject _gameOverPanel;
    [SerializeField] private Button _restartButton;
    [SerializeField] private Button _menuButton; 

    [SerializeField] private string _menuSceneName = "MainMenu";

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        _gameOverPanel.SetActive(false);
        _restartButton.onClick.AddListener(Restart);
        if (_menuButton != null)
            _menuButton.onClick.AddListener(GoToMenu);
    }

    public void ShowGameOver()
    {
        AudioBeatDetector.Instance?.StopMusic();
        _gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
        AudioSource audio = FindAnyObjectByType<AudioSource>();
        if (audio != null) audio.Stop();
    }

    private void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void GoToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(_menuSceneName);
    }
}