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

    [Header("Botón Pausa Mobile")]
    [SerializeField] private Button _pauseButton;

    [Header("Configuración (Settings)")]
    [SerializeField] private GameObject _settingPanel; // El panel de Configuración

    [Header("Controles Mobile")]
    [SerializeField] private GameObject _mobileControls; // Joystick + botones touch

    [SerializeField] private string _menuSceneName = "MainMenu";

    private bool _isPaused = false;
    private bool _isInSettings = false;
    private AudioSource _musicSource;

    void Start()
    {
        _pausePanel.SetActive(false);

        if (AudioBeatDetector.Instance != null)
            _musicSource = AudioBeatDetector.Instance.GetComponent<AudioSource>();

        if (_resumeButton != null) _resumeButton.onClick.AddListener(Resume);
        if (_restartButton != null) _restartButton.onClick.AddListener(Restart);
        if (_menuButton != null) _menuButton.onClick.AddListener(GoToMenu);

        if (_pauseButton != null) _pauseButton.onClick.AddListener(OnPauseButtonPressed);
    }

    void Update()
    {
        if (!_isPaused && Time.timeScale == 0f) return;

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            OnPauseButtonPressed();
    }

    // Llamado por el botón táctil O por ESC.
    // Si estamos en el panel de settings, no hace nada (ese botón no debe reanudar)
    private void OnPauseButtonPressed()
    {
        if (_isInSettings) return;
        TogglePause();
    }

    public void TogglePause()
    {
        if (_isPaused) Resume();
        else Pause();
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
        Time.timeScale = 1f;

        if (_musicSource != null)
            _musicSource.UnPause();
    }

    // Llamado por Btn_Configuracion: abre settings y oculta pausa + controles mobile
    public void OpenSettings()
    {
        _isInSettings = true;
        if (_pausePanel != null) _pausePanel.SetActive(false);
        if (_mobileControls != null) _mobileControls.SetActive(false);
    }

    // Llamado por Btn_Back dentro de settings: cierra settings y vuelve a pausa
    public void CloseSettings()
    {
        _isInSettings = false;
        if (_settingPanel != null) _settingPanel.SetActive(false);
        if (_pausePanel != null) _pausePanel.SetActive(true);
        if (_mobileControls != null) _mobileControls.SetActive(true);
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