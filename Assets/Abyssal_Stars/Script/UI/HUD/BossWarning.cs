using System.Collections;
using UnityEngine;

public class BossWarningUI : MonoBehaviour
{
    public static BossWarningUI Instance { get; private set; }

    [Header("Referencias de UI")]
    [SerializeField] private GameObject _visualGroup;

    [Header("Configuración de Audio")]
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _warningSiren;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (_visualGroup != null) _visualGroup.SetActive(false);

        if (_audioSource == null) _audioSource = GetComponent<AudioSource>();
    }

    public void ShowWarning(float duration)
    {
        StartCoroutine(WarningRoutine(duration));
    }

    private IEnumerator WarningRoutine(float duration)
    {
        if (_visualGroup != null) _visualGroup.SetActive(true);

        if (_audioSource != null && _warningSiren != null)
        {
            _audioSource.clip = _warningSiren;
            _audioSource.loop = true;
            _audioSource.Play();
        }

        yield return new WaitForSeconds(duration);

        if (_visualGroup != null) _visualGroup.SetActive(false);
        if (_audioSource != null) _audioSource.Stop();
    }
}