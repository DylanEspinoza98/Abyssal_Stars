using System.Collections;
using UnityEngine;

public class BossWarningUI : MonoBehaviour
{
    public static BossWarningUI Instance { get; private set; }

    [Header("Referencias de UI")]
    [SerializeField] private GameObject _visualGroup;

    [Header("Música de Fondo")]
    [SerializeField] private AudioSource _backgroundMusic;
    [SerializeField][Range(0f, 1f)] private float _duckedVolume = 0.15f;

    [Header("Sirena")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _warningSiren;

    private float _originalMusicVolume;
    private Coroutine _activeRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (_audioSource == null) _audioSource = GetComponent<AudioSource>();

        if (_backgroundMusic != null)
            _originalMusicVolume = _backgroundMusic.volume;

        HideImmediate();
    }

    private void OnEnable()
    {
        BossPhaseController.OnBossWarning += HandleWarning;
        BossPhaseController.OnBossFightStarted += HandleBossStarted;
    }

    private void OnDisable()
    {
        BossPhaseController.OnBossWarning -= HandleWarning;
        BossPhaseController.OnBossFightStarted -= HandleBossStarted;
    }

    private void HandleWarning()
    {
        // Leer duración directo del BossPhaseController
        float duration = BossPhaseController.Instance != null
            ? BossPhaseController.Instance.WarningDuration
            : 3f;

        ShowWarning(duration);
    }

    private void HandleBossStarted()
    {
        if (_activeRoutine != null) StopCoroutine(_activeRoutine);
        HideImmediate();
    }

    public void ShowWarning(float duration)
    {
        if (_activeRoutine != null) StopCoroutine(_activeRoutine);
        _activeRoutine = StartCoroutine(WarningRoutine(duration));
    }

    private IEnumerator WarningRoutine(float duration)
    {
        if (_visualGroup != null) _visualGroup.SetActive(true);

        if (_backgroundMusic != null)
            _backgroundMusic.volume = _duckedVolume;

        if (_audioSource != null && _warningSiren != null)
        {
            _audioSource.clip = _warningSiren;
            _audioSource.loop = true;
            _audioSource.Play();
        }

        yield return new WaitForSeconds(duration);

        HideImmediate();
        _activeRoutine = null;
    }
    private void HideImmediate()
    {
        if (_visualGroup != null) _visualGroup.SetActive(false);
        if (_audioSource != null) _audioSource.Stop();

        if (_backgroundMusic != null)
            _backgroundMusic.volume = _originalMusicVolume;
    }
}