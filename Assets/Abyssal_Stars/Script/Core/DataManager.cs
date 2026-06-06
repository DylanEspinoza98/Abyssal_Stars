using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class SettingsData
{
    public float musicVolume = 1f;
    public float sfxVolume = 1f;

    public bool vSync = true;
    public bool muteOnUnfocus = true;
    public bool showFPS = false;

    public string moveUpKey = "W";
    public string moveDownKey = "S";
    public string moveLeftKey = "A";
    public string moveRightKey = "D";
    public string shootKey = "Space";
    public string bombKey = "B";
    public string focusKey = "LeftShift";
}

[System.Serializable]
public class ProgressionData
{
    public List<string> unlockedLevels = new List<string> { "Level_1" };
}

[System.Serializable]
public class GameSaveData
{
    public ScoreboardSaveData scoreboard = new ScoreboardSaveData();
    public SettingsData settings = new SettingsData();
    public ProgressionData progression = new ProgressionData();
}

public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }

    private const string SAVE_KEY = "BulletHell_MasterSaveData";

    public GameSaveData SaveData { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadGame();
        }
        else Destroy(gameObject);
    }

    public void LoadGame()
    {
        if (PlayerPrefs.HasKey(SAVE_KEY))
        {
            string json = PlayerPrefs.GetString(SAVE_KEY);
            SaveData = JsonUtility.FromJson<GameSaveData>(json);
        }
        else
        {
            SaveData = new GameSaveData();
        }

        QualitySettings.vSyncCount = SaveData.settings.vSync ? 1 : 0;
    }

    public void SaveGame()
    {
        string json = JsonUtility.ToJson(SaveData);
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (SaveData == null) return;

        if (SaveData.settings.muteOnUnfocus)
        {
            AudioListener.pause = !hasFocus;
        }
        else
        {
            AudioListener.pause = false;
        }
    }
}