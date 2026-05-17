using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


// Se mantiene entre escenas y le da sonido a los botones del menu, asegurando que cada boton tenga el sonido asignado al cargar una nueva escena
public class ButtonSoundManager : MonoBehaviour
{
    public static ButtonSoundManager Instance { get; private set; }

    [Header("Sonido")]
    [SerializeField] private AudioClip _buttonSound;
    [SerializeField] private float _volume = 1f;

    private AudioSource _audioSource;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.playOnAwake = false;
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        
        ConnectAllButtons();
    }

    void Start()
    {
        
        ConnectAllButtons();
    }

    private void ConnectAllButtons()
    {
        Button[] allButtons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Button btn in allButtons)
        {
            // Evita agregar el listener mas de una vez
            btn.onClick.RemoveListener(PlayButtonSound);
            btn.onClick.AddListener(PlayButtonSound);
        }
    }

    private void PlayButtonSound()
    {
        if (_buttonSound != null)
            _audioSource.PlayOneShot(_buttonSound, _volume);
    }
}