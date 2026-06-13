using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class PlayerBomb : MonoBehaviour
{
    public static PlayerBomb Instance { get; private set; }

    [Header("Inventario de Bombas")]
    [SerializeField] private int _startingBombs = 2;
    [SerializeField] private int _maxBombs = 5;

    public int CurrentBombs { get; private set; }
    public bool HasUsedBomb { get; private set; } = false;

    public event Action<int> OnBombsChanged;

    [Header("Feedback Visual y Sonoro")]
    [SerializeField] private AudioClip _bombSound;
    [Tooltip("Arrastra aqu� tu Prefab del humo expansivo")]
    [SerializeField] private GameObject _smokeEffectPrefab;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        CurrentBombs = _startingBombs;
    }

    void Update()
    {
        if (Time.timeScale == 0f) return;

        if (PlayerHealth.Instance != null && PlayerHealth.Instance.IsDead) return;

        if (Keyboard.current != null && DataManager.Instance != null)
        {
            SettingsData settings = DataManager.Instance.SaveData.settings;

            if (WasKeyPressedThisFrame(settings.bombKey))
            {
                TryUseBomb();
            }
        }
    }

    public void TryUseBomb()
    {
        if (CurrentBombs > 0)
        {
            CurrentBombs--;
            HasUsedBomb = true;

            OnBombsChanged?.Invoke(CurrentBombs);

            ClearScreen();
            PlayFeedback();

            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.ResetMultiplierToOne();
                ScoreManager.Instance.ActivateBlackHole(0.5f);
            }
        }
    }

    public void AddBomb()
    {
        if (CurrentBombs < _maxBombs)
        {
            CurrentBombs++;
            OnBombsChanged?.Invoke(CurrentBombs);
        }
    }

    private void ClearScreen()
    {
        EnemyBullet[] activeBullets = FindObjectsByType<EnemyBullet>();
        foreach (EnemyBullet bullet in activeBullets)
        {
            bullet.ReturnToPool();
        }

        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in enemies)
        {
            Destroy(enemy);
        }
    }

    private void PlayFeedback()
    {
        if (_bombSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(_bombSound);
        }

        if (_smokeEffectPrefab != null)
        {
            Instantiate(_smokeEffectPrefab, transform.position, Quaternion.identity);
        }
    }
    private bool WasKeyPressedThisFrame(string keyName)
    {
        if (string.IsNullOrEmpty(keyName) || Keyboard.current == null) return false;

        foreach (var key in Keyboard.current.allKeys)
        {
            if (key.name.Equals(keyName, StringComparison.OrdinalIgnoreCase))
            {
                return key.wasPressedThisFrame;
            }
        }
        return false;
    }
}