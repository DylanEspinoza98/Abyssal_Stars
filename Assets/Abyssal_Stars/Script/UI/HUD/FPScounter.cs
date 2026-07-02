using UnityEngine;
using TMPro;

public class FPSCounter : MonoBehaviour
{
    private TextMeshProUGUI _fpsText;
    private float _timer;
    private float _refreshRate = 0.5f;

    private void Awake()
    {
        _fpsText = GetComponent<TextMeshProUGUI>();
    }

    private void Start()
    {
        UpdateVisibility();
    }

    public void UpdateVisibility()
    {
        if (DataManager.Instance != null && _fpsText != null)
        {
            _fpsText.enabled = DataManager.Instance.SaveData.settings.showFPS;
        }
    }

    private void Update()
    {
        if (_fpsText.enabled && Time.unscaledDeltaTime > 0)
        {
            _timer += Time.unscaledDeltaTime;
            if (_timer >= _refreshRate)
            {
                int fps = Mathf.RoundToInt(1f / Time.unscaledDeltaTime);
                _fpsText.text = $"FPS: {fps}";
                _timer = 0f;
            }
        }
    }
}