using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SettingsMenuUI : MonoBehaviour
{
    [Header("Paneles Modulares")]
    [SerializeField] private GameObject _audioPanel;
    [SerializeField] private GameObject _pcPanel;
    [SerializeField] private GameObject _controlsPanel;

    [Header("Referencias de Audio")]
    [SerializeField] private Slider _musicSlider;
    [SerializeField] private Slider _sfxSlider;

    [Header("Referencias de PC")]
    [SerializeField] private Toggle _vSyncToggle;
    [SerializeField] private Toggle _muteUnfocusToggle;
    [SerializeField] private Toggle _fpsToggle;

    [Header("Textos de Controles")]
    [SerializeField] private TextMeshProUGUI _txtUp;
    [SerializeField] private TextMeshProUGUI _txtDown;
    [SerializeField] private TextMeshProUGUI _txtLeft;
    [SerializeField] private TextMeshProUGUI _txtRight;
    [SerializeField] private TextMeshProUGUI _txtShoot;
    [SerializeField] private TextMeshProUGUI _txtBomb;
    [SerializeField] private TextMeshProUGUI _txtFocus;

    [Header("Zona de Peligro (Borrar Datos)")]
    [SerializeField] private GameObject _confirmDeletePanel;
    [SerializeField] private TextMeshProUGUI _feedbackText;

    private bool _isRebinding = false;
    private string _actionToRebind = "";
    private bool _isAudioOnlyMode = false;
    private TextMeshProUGUI _currentTextToUpdate;

    private void Start()
    {
        if (DataManager.Instance == null) return;

        SettingsData settings = DataManager.Instance.SaveData.settings;

        if (_musicSlider != null)
        {
            _musicSlider.value = settings.musicVolume;
            _musicSlider.onValueChanged.AddListener(SetMusicVolume);
        }

        if (_sfxSlider != null)
        {
            _sfxSlider.value = settings.sfxVolume;
            _sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        }

        if (_vSyncToggle != null)
        {
            _vSyncToggle.isOn = settings.vSync;
            _vSyncToggle.onValueChanged.AddListener(SetVSync);
        }

        if (_muteUnfocusToggle != null)
        {
            _muteUnfocusToggle.isOn = settings.muteOnUnfocus;
            _muteUnfocusToggle.onValueChanged.AddListener(SetMuteOnUnfocus);
        }

        if (_fpsToggle != null)
        {
            _fpsToggle.isOn = settings.showFPS;
            _fpsToggle.onValueChanged.AddListener(SetShowFPS);
        }

        SetMusicVolume(settings.musicVolume);
        SetSFXVolume(settings.sfxVolume);

        UpdateControlTexts();
    }

    private void Update()
    {
        // Keyboard.current se usa únicamente aquí para capturar la tecla
        // durante el flujo de reasignación (rebinding). No es input de gameplay.
        if (_isRebinding && Keyboard.current != null)
        {
            if (Keyboard.current.anyKey.wasPressedThisFrame)
            {
                foreach (var key in Keyboard.current.allKeys)
                {
                    if (key.wasPressedThisFrame)
                    {
                        if (key.name == "escape") return;
                        // Guardar el nombre canónico de InputSystem (ej. "w", "leftShift")
                        AssignKey(key.name);
                        break;
                    }
                }
            }
        }
    }

    public void ShowFullMenu()
    {
        _isAudioOnlyMode = false;
        gameObject.SetActive(true);

        if (_audioPanel != null) _audioPanel.SetActive(true);
        if (_pcPanel != null) _pcPanel.SetActive(true);
        if (_controlsPanel != null) _controlsPanel.SetActive(false);
    }

    public void ShowAudioOnly()
    {
        _isAudioOnlyMode = true;
        gameObject.SetActive(true);

        if (_audioPanel != null) _audioPanel.SetActive(true);
        if (_pcPanel != null) _pcPanel.SetActive(false);
        if (_controlsPanel != null) _controlsPanel.SetActive(false);
    }

    public void OpenControlsPanel()
    {
        if (_audioPanel != null) _audioPanel.SetActive(false);
        if (_pcPanel != null) _pcPanel.SetActive(false);
        if (_controlsPanel != null) _controlsPanel.SetActive(true);
    }

    public void CloseControlsPanel()
    {
        if (_controlsPanel != null) _controlsPanel.SetActive(false);
        if (_audioPanel != null) _audioPanel.SetActive(true);

        if (_pcPanel != null) _pcPanel.SetActive(!_isAudioOnlyMode);
    }

    public void StartRebindUp() => StartRebind("Up", _txtUp);
    public void StartRebindDown() => StartRebind("Down", _txtDown);
    public void StartRebindLeft() => StartRebind("Left", _txtLeft);
    public void StartRebindRight() => StartRebind("Right", _txtRight);
    public void StartRebindShoot() => StartRebind("Shoot", _txtShoot);
    public void StartRebindBomb() => StartRebind("Bomb", _txtBomb);
    public void StartRebindFocus() => StartRebind("Focus", _txtFocus);

    private void StartRebind(string actionName, TextMeshProUGUI textElement)
    {
        _actionToRebind = actionName;
        _currentTextToUpdate = textElement;
        _isRebinding = true;

        if (_currentTextToUpdate != null)
        {
            _currentTextToUpdate.text = "[ ... ]";
        }
    }

    private void AssignKey(string newKeyName)
    {
        _isRebinding = false;
        SettingsData settings = DataManager.Instance.SaveData.settings;

        switch (_actionToRebind)
        {
            case "Up": settings.moveUpKey = newKeyName; break;
            case "Down": settings.moveDownKey = newKeyName; break;
            case "Left": settings.moveLeftKey = newKeyName; break;
            case "Right": settings.moveRightKey = newKeyName; break;
            case "Shoot": settings.shootKey = newKeyName; break;
            case "Bomb": settings.bombKey = newKeyName; break;
            case "Focus": settings.focusKey = newKeyName; break;
        }

        UpdateControlTexts();
        DataManager.Instance.SaveGame();

        // Notificar al InputManager para que aplique el nuevo binding al vuelo
        InputManager.Instance?.RefreshBindings();
    }

    private void UpdateControlTexts()
    {
        if (DataManager.Instance == null) return;
        SettingsData settings = DataManager.Instance.SaveData.settings;

        if (_txtUp != null)    _txtUp.text    = DisplayKey(settings.moveUpKey);
        if (_txtDown != null)  _txtDown.text  = DisplayKey(settings.moveDownKey);
        if (_txtLeft != null)  _txtLeft.text  = DisplayKey(settings.moveLeftKey);
        if (_txtRight != null) _txtRight.text = DisplayKey(settings.moveRightKey);
        if (_txtShoot != null) _txtShoot.text = DisplayKey(settings.shootKey);
        if (_txtBomb != null)  _txtBomb.text  = DisplayKey(settings.bombKey);
        if (_txtFocus != null) _txtFocus.text = DisplayKey(settings.focusKey);
    }

    /// <summary>
    /// Convierte el nombre canónico de InputSystem a texto legible para el UI.
    /// "w" → "W",  "leftShift" → "Left Shift",  "space" → "Space".
    /// </summary>
    private string DisplayKey(string keyName)
    {
        if (string.IsNullOrEmpty(keyName)) return "—";

        // Insertar espacio antes de mayúsculas internas: "leftShift" → "left Shift"
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append(char.ToUpper(keyName[0]));
        for (int i = 1; i < keyName.Length; i++)
        {
            if (char.IsUpper(keyName[i])) sb.Append(' ');
            sb.Append(keyName[i]);
        }
        return sb.ToString(); // "Left Shift", "W", "Space", "B"
    }

    public void SetMusicVolume(float sliderValue)
    {
        DataManager.Instance.SaveData.settings.musicVolume = sliderValue;
        DataManager.Instance.SaveGame();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(sliderValue);
        }
    }

    public void SetSFXVolume(float sliderValue)
    {
        DataManager.Instance.SaveData.settings.sfxVolume = sliderValue;
        DataManager.Instance.SaveGame();

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetSFXVolume(sliderValue);
        }
    }

    public void SetVSync(bool isOn)
    {
        QualitySettings.vSyncCount = isOn ? 1 : 0;
        if (DataManager.Instance != null)
        {
            DataManager.Instance.SaveData.settings.vSync = isOn;
            DataManager.Instance.SaveGame();
        }
    }

    public void SetMuteOnUnfocus(bool isOn)
    {
        if (DataManager.Instance != null)
        {
            DataManager.Instance.SaveData.settings.muteOnUnfocus = isOn;
            DataManager.Instance.SaveGame();
        }
    }

    public void SetShowFPS(bool isOn)
    {
        if (DataManager.Instance != null)
        {
            DataManager.Instance.SaveData.settings.showFPS = isOn;
            DataManager.Instance.SaveGame();

            FPSCounter[] counters = FindObjectsByType<FPSCounter>(FindObjectsInactive.Include);
            foreach (var counter in counters)
            {
                counter.UpdateVisibility();
            }
        }
    }
    public void PromptDeleteScores()
    {
        if (_confirmDeletePanel != null) _confirmDeletePanel.SetActive(true);
        if (_feedbackText != null) _feedbackText.text = "";
    }
    public void CancelDeleteScores()
    {
        if (_confirmDeletePanel != null) _confirmDeletePanel.SetActive(false);
    }
    public void ConfirmDeleteScores()
    {
        if (ScoreboardManager.Instance != null)
        {
            ScoreboardManager.Instance.ClearAllScores();

            if (_feedbackText != null)
            {
                _feedbackText.text = "�Puntajes borrados con �xito!";
                _feedbackText.color = Color.green;
            }

            Invoke(nameof(CancelDeleteScores), 1.5f);
        }
    }
}