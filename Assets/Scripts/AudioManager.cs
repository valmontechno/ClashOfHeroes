using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    private AudioSource audioSource;

    public AudioClip selectPawnSound;
    public AudioClip deselectPawnSound;

    public AudioClip destroyPawnSound;
    public AudioClip movePawnSound;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            audioSource = GetComponent<AudioSource>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Play(AudioClip clip, float volume = 1.0f)
    {
        Instance.audioSource.PlayOneShot(clip, volume);
    }
}
