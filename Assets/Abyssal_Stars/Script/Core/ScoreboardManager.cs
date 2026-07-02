using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class ScoreEntry
{
    public int score;
    public string rank;
}

[System.Serializable]
public class LevelScoreData
{
    public string levelID;
    public List<ScoreEntry> highScores = new List<ScoreEntry>();
}

[System.Serializable]
public class ScoreboardSaveData
{
    public List<LevelScoreData> levelScores = new List<LevelScoreData>();
}

public class ScoreboardManager : MonoBehaviour
{
    public static ScoreboardManager Instance { get; private set; }

    [SerializeField] private int _maxScoresToKeep = 5;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    public void AddScore(string levelID, int newScore, string newRank)
    {
        var allScores = DataManager.Instance.SaveData.scoreboard.levelScores;

        LevelScoreData levelData = allScores.FirstOrDefault(l => l.levelID == levelID);

        if (levelData == null)
        {
            levelData = new LevelScoreData { levelID = levelID };
            allScores.Add(levelData);
        }

        levelData.highScores.Add(new ScoreEntry { score = newScore, rank = newRank });

        levelData.highScores = levelData.highScores
            .OrderByDescending(s => s.score)
            .Take(_maxScoresToKeep)
            .ToList();

        DataManager.Instance.SaveGame();
    }

    public List<ScoreEntry> GetScoresForLevel(string levelID)
    {
        var allScores = DataManager.Instance.SaveData.scoreboard.levelScores;
        LevelScoreData levelData = allScores.FirstOrDefault(l => l.levelID == levelID);
        return levelData != null ? levelData.highScores : new List<ScoreEntry>();
    }

    public void ClearAllScores()
    {
        if (DataManager.Instance != null)
        {
            DataManager.Instance.SaveData.scoreboard.levelScores.Clear();

            DataManager.Instance.SaveGame();

            Debug.Log("Scoreboard: Todos los puntajes han sido borrados.");
        }
    }
}