using UnityEngine;
using System.Collections;

public class MenuMusicController : MonoBehaviour
{
    [SerializeField] private AudioClip menuMusic;

    IEnumerator Start()
    {
        while (AudioManager.Instance == null) yield return null;

        if (menuMusic != null)
        {
            AudioManager.Instance.PlayMusic(menuMusic);
        }
    }
}