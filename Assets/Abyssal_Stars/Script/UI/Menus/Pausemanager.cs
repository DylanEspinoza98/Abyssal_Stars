using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    [Header("UI Principal")]
    [SerializeField] private GameObject _pausePanel;
    [SerializeField] private Button _resumeButton;
    [SerializeField] private Button _settingsButton;
    [SerializeField] private Button _restartButton;
    [SerializeField] private Button _menuButton;

    [Header("UI Configuración (PC)")]
    [SerializeField] private SettingsMenuUI _settingsMenu;

    [Header("Botón Pausa Mobile")]
    [SerializeField] private Button _pauseButton;

    [Header("Configuración (Settings) Mobile")]
    [SerializeField] private GameObject _settingPanel; // El panel de Configuración

    [Header("Controles Mobile")]
    [SerializeField] private GameObject _mobileControls; // Joystick + botones touch

    [SerializeField] private string _menuSceneName = "MainMenu";

    private bool _isPaused = false;
    private bool _isInSettings = false;

    private bool IsMobile =>
        MobileInputManager.Instance != null && MobileInputManager.Instance.IsMobileActive;

    void Start()
    {
        _pausePanel.SetActive(false);

        if (_settingsMenu != null) _settingsMenu.gameObject.SetActive(false);

        if (_resumeButton != null) _resumeButton.onClick.AddListener(Resume);
        if (_restartButton != null) _restartButton.onClick.AddListener(Restart);
        if (_menuButton != null) _menuButton.onClick.AddListener(GoToMenu);

        if (_settingsButton != null) _settingsButton.onClick.AddListener(OpenSettings);
        if (_pauseButton != null) _pauseButton.onClick.AddListener(OnPauseButtonPressed);
    }

    void Update()
    {
        if (!_isPaused && Time.timeScale == 0f) return;

        if (InputManager.Instance != null && InputManager.Instance.Pause.WasPressedThisFrame())
            OnPauseButtonPressed();
    }

    // Llamado por el botón táctil O por la tecla de pausa (ESC).
    private void OnPauseButtonPressed()
    {
        // Si el menú de configuración de PC está abierto, la tecla lo cierra
        if (_settingsMenu != null && _settingsMenu.gameObject.activeSelf)
        {
            CloseSettings();
            return;
        }

        // Si estamos en el panel de settings mobile, no hace nada (ese botón no debe reanudar)
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

        if (AudioBeatDetector.Instance != null)
            AudioBeatDetector.Instance.PauseMusic();
    }

    public void Resume()
    {
        _isPaused = false;
        _pausePanel.SetActive(false);

        if (_settingsMenu != null) _settingsMenu.gameObject.SetActive(false);

        Time.timeScale = 1f;

        if (AudioBeatDetector.Instance != null)
            AudioBeatDetector.Instance.ResumeMusic();
    }

    // Llamado por Btn_Configuracion (o el botón de settings de PC):
    // abre settings y oculta pausa + controles mobile
    public void OpenSettings()
    {
        _isInSettings = true;
        if (_pausePanel != null) _pausePanel.SetActive(false);
        if (_mobileControls != null) _mobileControls.SetActive(false);

        if (_settingsMenu != null) _settingsMenu.ShowAudioOnly();
    }

    // Llamado por Btn_Back dentro de settings: cierra settings y vuelve a pausa
    public void CloseSettings()
    {
        _isInSettings = false;

        if (_settingsMenu != null) _settingsMenu.gameObject.SetActive(false);
        if (_settingPanel != null) _settingPanel.SetActive(false);
        if (_pausePanel != null) _pausePanel.SetActive(true);

        // Solo re-activar los controles touch si realmente estamos en mobile
        if (IsMobile && _mobileControls != null) _mobileControls.SetActive(true);
    }

    private void Restart()
    {
        _isPaused = false;
        Time.timeScale = 1f;
        if (AudioBeatDetector.Instance != null) AudioBeatDetector.Instance.StopMusic();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void GoToMenu()
    {
        _isPaused = false;
        Time.timeScale = 1f;
        if (AudioBeatDetector.Instance != null) AudioBeatDetector.Instance.StopMusic();
        SceneManager.LoadScene(_menuSceneName);
    }
}
