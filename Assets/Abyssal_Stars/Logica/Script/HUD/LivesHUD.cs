using UnityEngine;
using UnityEngine.UI;

public class LivesHUD : MonoBehaviour
{
    [SerializeField] private playerScript _player;
    [SerializeField] private Image[] _heartImages;

    private int _lastLives = -1;

    void Update()
    {
        int currentLives = _player.TotalLives;
        if (currentLives == _lastLives) return;

        _lastLives = currentLives;
        for (int i = 0; i < _heartImages.Length; i++)
        {
            _heartImages[i].enabled = i < currentLives;
        }
    }
}