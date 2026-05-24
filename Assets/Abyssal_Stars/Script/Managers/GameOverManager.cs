using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject _gameOverPanel;
    [SerializeField] private Button _restartButton;
    [SerializeField] private Button _menuButton;

    [Header("Audio")]
    [SerializeField] private AudioClip _gameOverMusic;
    [SerializeField] private AudioSource _gameOverAudioSource;

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

        
        if (_gameOverAudioSource != null && _gameOverMusic != null)
        {
            _gameOverAudioSource.clip = _gameOverMusic;
            _gameOverAudioSource.loop = true;
            _gameOverAudioSource.ignoreListenerPause = true;
            _gameOverAudioSource.Play();
        }
    }

    private void StopGameOverMusic()
    {
        if (_gameOverAudioSource != null && _gameOverAudioSource.isPlaying)
            _gameOverAudioSource.Stop();
    }

    private void Restart()
    {
        StopGameOverMusic();
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void GoToMenu()
    {
        StopGameOverMusic();
        Time.timeScale = 1f;
        SceneManager.LoadScene(_menuSceneName);
    }
}