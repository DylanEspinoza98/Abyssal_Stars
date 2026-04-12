using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverManager : MonoBehaviour
{
    [SerializeField] private GameObject _gameOverPanel;
    [SerializeField] private Button _restartButton;

    void Start()
    {
        _gameOverPanel.SetActive(false);
        _restartButton.onClick.AddListener(Restart);
    }

    public void ShowGameOver()
    {
        _gameOverPanel.SetActive(true);
        Time.timeScale = 0f;

        AudioSource audio = FindObjectOfType<AudioSource>();
        if (audio != null) audio.Stop();
    }

    private void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(
          SceneManager.GetActiveScene().name);
    }
}