using UnityEngine;
using System;
using System.Collections;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    public int CurrentScore { get; private set; }

    [Header("Multiplicador de Supervivencia")]
    [SerializeField] private float _timeForTier1 = 30f;
    [SerializeField] private float _timeForTier2 = 60f;
    [SerializeField] private float _timeForTier3 = 90f;

    [Header("Mecánicas de Bomba")]
    [Tooltip("Puntos necesarios para ganar una bomba extra")]
    [SerializeField] private int _scoreForNextBomb = 20000;

    private int _scoreAccumulatedForBomb = 0;
    public bool IsBlackHoleActive { get; private set; } = false;

    private float _survivalTimer = 0f;
    public float CurrentMultiplier { get; private set; } = 1.0f;

    public event Action<int> OnScoreChanged;
    public event Action<float> OnMultiplierChanged;
    public event Action<bool> OnBlackHoleToggled;

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
        if (PlayerHealth.Instance != null)
        {
            PlayerHealth.Instance.OnPlayerDied += ResetSurvivalStreak;
        }
    }

    void OnDestroy()
    {
        if (PlayerHealth.Instance != null)
        {
            PlayerHealth.Instance.OnPlayerDied -= ResetSurvivalStreak;
        }
    }

    void Update()
    {
        if (PlayerHealth.Instance != null && !PlayerHealth.Instance.IsDead)
        {
            _survivalTimer += Time.deltaTime;
            CheckMultiplierThresholds();
        }
    }

    private void CheckMultiplierThresholds()
    {
        if (_survivalTimer >= _timeForTier3 && CurrentMultiplier < 4.0f)
        {
            CurrentMultiplier = 4.0f;
            OnMultiplierChanged?.Invoke(CurrentMultiplier);
        }
        else if (_survivalTimer >= _timeForTier2 && _survivalTimer < _timeForTier3 && CurrentMultiplier < 2.0f)
        {
            CurrentMultiplier = 2.0f;
            OnMultiplierChanged?.Invoke(CurrentMultiplier);
        }
        else if (_survivalTimer >= _timeForTier1 && _survivalTimer < _timeForTier2 && CurrentMultiplier < 1.5f)
        {
            CurrentMultiplier = 1.5f;
            OnMultiplierChanged?.Invoke(CurrentMultiplier);
        }
    }

    private void ResetSurvivalStreak()
    {
        _survivalTimer = 0f;

        if (CurrentMultiplier > 1.0f)
        {
            CurrentMultiplier = 1.0f;
            OnMultiplierChanged?.Invoke(CurrentMultiplier);
        }

        OnScoreChanged?.Invoke(CurrentScore);
    }

    public void AddScore(int points)
    {
        if (IsBlackHoleActive) return;

        int finalPoints = Mathf.RoundToInt(points * CurrentMultiplier);
        CurrentScore += finalPoints;

        OnScoreChanged?.Invoke(CurrentScore);

        _scoreAccumulatedForBomb += finalPoints;
        if (_scoreAccumulatedForBomb >= _scoreForNextBomb)
        {
            _scoreAccumulatedForBomb -= _scoreForNextBomb;

            if (PlayerBomb.Instance != null)
            {
                PlayerBomb.Instance.AddBomb();
            }
        }
    }

    public void ResetScore()
    {
        CurrentScore = 0;
        _scoreAccumulatedForBomb = 0;
        ResetSurvivalStreak();
        OnScoreChanged?.Invoke(CurrentScore);
    }

    public void ResetMultiplierToOne()
    {
        ResetSurvivalStreak();
    }

    public void ActivateBlackHole(float duration)
    {
        StartCoroutine(BlackHoleRoutine(duration));
    }

    private IEnumerator BlackHoleRoutine(float duration)
    {
        IsBlackHoleActive = true;
        OnBlackHoleToggled?.Invoke(true);

        yield return new WaitForSeconds(duration);

        IsBlackHoleActive = false;
        OnBlackHoleToggled?.Invoke(false);
    }
}