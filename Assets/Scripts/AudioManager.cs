using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    private AudioSource audioSource;

    [SerializeField] private AudioClip destroyPawnSound;
    [SerializeField] private AudioClip movePawnSound;

    public void PlayDestroyPawnSound() => PlaySound(destroyPawnSound);
    public void PlayMovePawnSound() => PlaySound(movePawnSound);

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

    public void PlaySound(AudioClip clip, float volume = 1.0f)
    {
        Instance.audioSource.PlayOneShot(clip, volume);
    }
}
