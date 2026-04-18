using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class VictoryManager : MonoBehaviour
{
    public static VictoryManager Instance { get; private set; }

    [SerializeField] private GameObject _victoryPanel;
    [SerializeField] private Button _menuButton;

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
        _menuButton.onClick.AddListener(GoToMenu);
    }

    public void ShowVictory()
    {
        _victoryPanel.SetActive(true);
        Time.timeScale = 0f;
        AudioSource audio = FindAnyObjectByType<AudioSource>();
        if (audio != null) audio.Stop();
    }

    private void GoToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}