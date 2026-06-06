using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class PauseManager : MonoBehaviour
{
    [Header("UI Principal")]
    [SerializeField] private GameObject _pausePanel;
    [SerializeField] private Button _resumeButton;
    [SerializeField] private Button _settingsButton; 
    [SerializeField] private Button _restartButton;
    [SerializeField] private Button _menuButton;

    [Header("UI Configuración")]
    [SerializeField] private SettingsMenuUI _settingsMenu;

    [SerializeField] private string _menuSceneName = "MainMenu";

    private bool _isPaused = false;
    private AudioSource _musicSource;

    void Start()
    {
        _pausePanel.SetActive(false);

        if (_settingsMenu != null) _settingsMenu.gameObject.SetActive(false);
        if (AudioBeatDetector.Instance != null)
            _musicSource = AudioBeatDetector.Instance.GetComponent<AudioSource>();

        if (_resumeButton != null) _resumeButton.onClick.AddListener(Resume);
        if (_restartButton != null) _restartButton.onClick.AddListener(Restart);
        if (_menuButton != null) _menuButton.onClick.AddListener(GoToMenu);

        if (_settingsButton != null) _settingsButton.onClick.AddListener(OpenSettings);
    }

    void Update()
    {
        if (!_isPaused && Time.timeScale == 0f) return;

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (_settingsMenu != null && _settingsMenu.gameObject.activeSelf)
            {
                CloseSettings();
            }
            else if (_isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    private void Pause()
    {
        _isPaused = true;
        _pausePanel.SetActive(true);
        Time.timeScale = 0f;

        if (_musicSource != null && _musicSource.isPlaying)
            _musicSource.Pause();
    }

    public void Resume()
    {
        _isPaused = false;
        _pausePanel.SetActive(false);

        if (_settingsMenu != null) _settingsMenu.gameObject.SetActive(false);

        Time.timeScale = 1f;

        if (_musicSource != null)
            _musicSource.UnPause();
    }


    private void OpenSettings()
    {
        _pausePanel.SetActive(false);
        if (_settingsMenu != null)
        {
            _settingsMenu.ShowAudioOnly();
        }
    }

    public void CloseSettings()
    {
        if (_settingsMenu != null)
        {
            _settingsMenu.gameObject.SetActive(false);
        }
        _pausePanel.SetActive(true); 
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