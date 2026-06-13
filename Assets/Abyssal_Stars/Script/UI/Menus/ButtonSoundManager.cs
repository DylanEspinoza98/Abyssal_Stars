using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ButtonSoundManager : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private AudioClip _buttonSound;

    void OnEnable() => SceneManager.sceneLoaded += OnSceneLoaded;
    void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => ConnectAllButtons();

    void Start() => ConnectAllButtons();

    private void ConnectAllButtons()
    {
        Button[] allButtons = FindObjectsByType<Button>(FindObjectsInactive.Include);
        foreach (Button btn in allButtons)
        {
            btn.onClick.RemoveListener(PlayButtonSound);
            btn.onClick.AddListener(PlayButtonSound);
        }
    }

    private void PlayButtonSound()
    {
        if (_buttonSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(_buttonSound);
        }
        else if (AudioManager.Instance == null)
        {
            Debug.LogWarning("ButtonSoundManager: No se encontró AudioManager en la escena.");
        }
    }
}