using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    private AudioSource audioSource;

    [SerializeField] private AudioClip moveSound;
    [SerializeField] private AudioClip collectSound;
    [SerializeField] private AudioClip attackSound;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            audioSource = gameObject.AddComponent<AudioSource>();
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayOneShot(AudioClip clip, float volume = 1.0f)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip, volume);
        }
    }

    public void PlayMove()
    {
        if (audioSource != null)
        {
            PlayOneShot(moveSound);
        }
    }

    public void PlayCollect()
    {
        if (audioSource != null)
        {
            PlayOneShot(collectSound, 0.5f);
        }
    }

    public void PlayAttack()
    {
        if (audioSource != null)
        {
            PlayOneShot(attackSound);
        }
    }
}