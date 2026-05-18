using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class VictoryManager : MonoBehaviour
{
    public static VictoryManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject _victoryPanel;
    [SerializeField] private TextMeshProUGUI _victoryText;    
    [SerializeField] private Button _nextLevelButton;         
    [SerializeField] private TextMeshProUGUI _nextLevelText;  
    [SerializeField] private Button _menuButton;              

    [Header("Configuración")]
    [SerializeField] private bool _isLastLevel = false;       
    [SerializeField] private string _nextLevelSceneName = ""; 
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
        _victoryPanel.SetActive(false);

        if (_isLastLevel)
        {
            // Nivel 3 (proximamente)
            if (_victoryText != null) _victoryText.text = "¡Gracias por Jugar!";
            if (_nextLevelButton != null) _nextLevelButton.gameObject.SetActive(false);
        }
        else
        {
            // Nivel 1 y 2
            if (_victoryText != null) _victoryText.text = "¡Nivel Completado!";
            if (_nextLevelButton != null)
            {
                _nextLevelButton.gameObject.SetActive(true);
                _nextLevelButton.onClick.AddListener(GoToNextLevel);
            }
            if (_nextLevelText != null) _nextLevelText.text = "Siguiente Nivel";
        }

        if (_menuButton != null)
        {
            
            TextMeshProUGUI menuText = _menuButton.GetComponentInChildren<TextMeshProUGUI>();
            if (menuText != null) menuText.text = "Menú Principal";
            _menuButton.onClick.AddListener(GoToMenu);
        }
    }

    public void ShowVictory()
    {
        _victoryPanel.SetActive(true);
        Time.timeScale = 0f;

        AudioSource audio = FindAnyObjectByType<AudioSource>();
        if (audio != null) audio.Stop();
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