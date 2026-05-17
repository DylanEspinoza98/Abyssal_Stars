using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    [Header("Botones del Menu Principal")]
    [SerializeField] private GameObject _btnStart;  // Btn_Start
    [SerializeField] private GameObject _btnQuit;   // Btn_Quit
    [SerializeField] private GameObject _titleText; // Title_Text

    [Header("Panel Selector de Nivel")]
    [SerializeField] private GameObject _levelSelectPanel; // LevelSelectPanel

    [Header("Nombres de Escenas")]
    [SerializeField] private string _level1SceneName = "Primer_Nivel";
    [SerializeField] private string _level2SceneName = "Segundo_Nivel";
    [SerializeField] private string _level3SceneName = "Tercer_Nivel";

    void Start()
    {
       
        if (_levelSelectPanel != null) _levelSelectPanel.SetActive(false);

        // Asegura que los botones del menu empiecen visibles
        if (_btnStart != null) _btnStart.SetActive(true);
        if (_btnQuit != null) _btnQuit.SetActive(true);
        if (_titleText != null) _titleText.SetActive(true);
    }

    // Botones del menu principal 
    public void OnClickStart()
    {
        
        if (_btnStart != null) _btnStart.SetActive(false);
        if (_btnQuit != null) _btnQuit.SetActive(false);
        if (_titleText != null) _titleText.SetActive(false);
        if (_levelSelectPanel != null) _levelSelectPanel.SetActive(true);
    }

    public void OnClickQuit()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    //  Botones del selector de nivel 
    public void OnClickLevel1()
    {
        SceneManager.LoadScene(_level1SceneName);
    }

    public void OnClickLevel2()
    {
        SceneManager.LoadScene(_level2SceneName);
    }

    public void OnClickLevel3()
    {
        SceneManager.LoadScene(_level3SceneName);
    }

    public void OnClickBack()
    {
        // Muestra los botones del menu y oculta el selector
        if (_btnStart != null) _btnStart.SetActive(true);
        if (_btnQuit != null) _btnQuit.SetActive(true);
        if (_titleText != null) _titleText.SetActive(true);
        if (_levelSelectPanel != null) _levelSelectPanel.SetActive(false);
    }
}
