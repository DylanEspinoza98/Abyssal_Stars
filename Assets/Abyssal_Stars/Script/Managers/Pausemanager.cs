using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject _pausePanel;
    [SerializeField] private Button _resumeButton;
    [SerializeField] private Button _restartButton;
    [SerializeField] private Button _menuButton;

    [SerializeField] private string _menuSceneName = "MainMenu";

    private bool _isPaused = false;
    private AudioSource _musicSource;

    void Start()
    {
        _pausePanel.SetActive(false);

        // Busca el AudioSource de la musica (el que tiene AudioBeatDetector)
        if (AudioBeatDetector.Instance != null)
            _musicSource = AudioBeatDetector.Instance.GetComponent<AudioSource>();

        if (_resumeButton != null) _resumeButton.onClick.AddListener(Resume);
        if (_restartButton != null) _restartButton.onClick.AddListener(Restart);
        if (_menuButton != null) _menuButton.onClick.AddListener(GoToMenu);
    }

    void Update()
    {
        // No permite pausar si el game over esta activo
        if (!_isPaused && Time.timeScale == 0f) return;

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (_isPaused) Resume();
            else Pause();
        }
    }

    private void Pause()
    {
        _isPaused = true;
        _pausePanel.SetActive(true);
        Time.timeScale = 0f;

        // Pausa la musica — AudioBeatDetector deja de detectar beats automaticamente
        if (_musicSource != null && _musicSource.isPlaying)
            _musicSource.Pause();
    }

    public void Resume()
    {
        _isPaused = false;
        _pausePanel.SetActive(false);
        Time.timeScale = 1f;

        // Reanuda la musica — AudioBeatDetector vuelve a detectar beats
        if (_musicSource != null)
            _musicSource.UnPause();
    }

    private void Restart()
    {
        _isPaused = false;
        Time.timeScale = 1f;
        if (_musicSource != null) _musicSource.Stop();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void GoToMenu()
    {
        _isPaused = false;
        Time.timeScale = 1f;
        if (_musicSource != null) _musicSource.Stop();
        SceneManager.LoadScene(_menuSceneName);
    }
}