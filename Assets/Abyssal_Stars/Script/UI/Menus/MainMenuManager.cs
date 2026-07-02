using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject _titleText;

    [Header("Botones del Menu Principal")]
    [SerializeField] private GameObject _btnStart;
    [SerializeField] private GameObject _btnScoreboard;
    [SerializeField] private GameObject _btnQuit;
    [SerializeField] private GameObject _btnSettings;

    [Header("Paneles Secundarios")]
    [SerializeField] private GameObject _levelSelectPanel;
    [SerializeField] private GameObject _scoreboardPanel;
    [SerializeField] private GameObject _settingsPanel;

    [Header("Nombres de Escenas")]
    [SerializeField] private string _level1SceneName = "Primer_Nivel";
    [SerializeField] private string _level2SceneName = "Segundo_Nivel";
    [SerializeField] private string _level3SceneName = "Tercer_Nivel";

    void Start()
    {
        if (_levelSelectPanel != null) _levelSelectPanel.SetActive(false);
        if (_scoreboardPanel != null) _scoreboardPanel.SetActive(false);
        if (_settingsPanel != null) _settingsPanel.SetActive(false);

        ShowMainMenu();
    }

    public void OnClickStart()
    {
        HideMainMenu();
        if (_levelSelectPanel != null) _levelSelectPanel.SetActive(true);
        if (_settingsPanel != null) _settingsPanel.SetActive(false);
    }

    public void OnClickScoreboard()
    {
        HideMainMenu();
        if (_scoreboardPanel != null) _scoreboardPanel.SetActive(true);
        if (_settingsPanel != null) _settingsPanel.SetActive(false);
    }

    public void OnClickBack()
    {
        if (_levelSelectPanel != null) _levelSelectPanel.SetActive(false);
        if (_scoreboardPanel != null) _scoreboardPanel.SetActive(false);
        if (_settingsPanel != null) _settingsPanel.SetActive(false);

        ShowMainMenu();
    }
    public void OnClickSettings()
    {
        HideMainMenu();
        if (_settingsPanel != null) _settingsPanel.SetActive(true);
    }

    public void OnClickQuit()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private void HideMainMenu()
    {
        if (_btnStart != null) _btnStart.SetActive(false);
        if (_btnScoreboard != null) _btnScoreboard.SetActive(false);
        if (_btnQuit != null) _btnQuit.SetActive(false);
        if (_titleText != null) _titleText.SetActive(false);
        if (_btnSettings != null) _btnSettings.SetActive(false);
    }

    private void ShowMainMenu()
    {
        if (_btnStart != null) _btnStart.SetActive(true);
        if (_btnScoreboard != null) _btnScoreboard.SetActive(true);
        if (_btnQuit != null) _btnQuit.SetActive(true);
        if (_titleText != null) _titleText.SetActive(true);
        if (_btnSettings != null) _btnSettings.SetActive(true);
    }
    public void OnClickLevel1() => SceneManager.LoadScene(_level1SceneName);
    public void OnClickLevel2() => SceneManager.LoadScene(_level2SceneName);
    public void OnClickLevel3() => SceneManager.LoadScene(_level3SceneName);
}