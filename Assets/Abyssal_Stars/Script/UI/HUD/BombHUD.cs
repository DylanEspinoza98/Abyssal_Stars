using UnityEngine;

public class BombHUD : MonoBehaviour
{
    [SerializeField] private RectTransform _bombsRect;
    [SerializeField] private float _iconWidth = 50f;

    void Start()
    {
        if (PlayerBomb.Instance != null)
        {
            PlayerBomb.Instance.OnBombsChanged += UpdateBombsDisplay;
            UpdateBombsDisplay(PlayerBomb.Instance.CurrentBombs);
        }
    }

    private void OnDestroy()
    {
        if (PlayerBomb.Instance != null)
        {
            PlayerBomb.Instance.OnBombsChanged -= UpdateBombsDisplay;
        }
    }

    private void UpdateBombsDisplay(int currentBombs)
    {
        if (_bombsRect != null)
        {
            _bombsRect.sizeDelta = new Vector2(currentBombs * _iconWidth, _bombsRect.sizeDelta.y);
        }
    }
}