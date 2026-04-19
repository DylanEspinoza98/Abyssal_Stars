using UnityEngine;

public class LivesHUD : MonoBehaviour
{
    [SerializeField] private RectTransform _livesRect;
    [SerializeField] private float _iconWidth = 50f;

    void Start()
    {
        if (PlayerHealth.Instance != null)
        {
            PlayerHealth.Instance.OnLivesChanged += UpdateLivesDisplay;

            UpdateLivesDisplay(PlayerHealth.Instance.TotalLives);
        }
    }

    private void OnDestroy()
    {
        if (PlayerHealth.Instance != null)
        {
            PlayerHealth.Instance.OnLivesChanged -= UpdateLivesDisplay;
        }
    }

    private void UpdateLivesDisplay(int currentLives)
    {
        if (_livesRect != null)
        {
            _livesRect.sizeDelta = new Vector2(currentLives * _iconWidth, _livesRect.sizeDelta.y);
        }
    }
}