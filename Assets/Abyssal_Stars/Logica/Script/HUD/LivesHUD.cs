using UnityEngine;

public class LivesHUD : MonoBehaviour
{
    [SerializeField] private playerScript _player;
    [SerializeField] private RectTransform _livesRect;
    [SerializeField] private float _iconWidth = 50f;
    private void OnEnable()
    {
        if (_player != null)
            _player.OnLivesChanged += UpdateLivesDisplay;
    }
    private void OnDisable()
    {
        if (_player != null)
            _player.OnLivesChanged -= UpdateLivesDisplay;
    }

    void Start()
    {
        UpdateLivesDisplay(_player.TotalLives);
    }

    private void UpdateLivesDisplay(int currentLives)
    {
        _livesRect.sizeDelta = new Vector2(currentLives * _iconWidth, _livesRect.sizeDelta.y);
    }
}