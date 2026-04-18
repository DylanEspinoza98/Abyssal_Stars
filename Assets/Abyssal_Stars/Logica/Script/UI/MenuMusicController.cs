using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MenuMusicController : MonoBehaviour
{
    [SerializeField] private AudioClip menuMusic;
    private AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.playOnAwake = false;

        if (menuMusic != null)
        {
            audioSource.clip = menuMusic;
            audioSource.Play();
        }
    }
}