using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(AudioSource))]
public sealed class GameAudioController : MonoBehaviour
{
    [Header("Audio Profile")]
    [SerializeField] private GameAudioProfile profile;
    [SerializeField, Range(0f, 1f)] private float masterVolume = 1f;

    [Header("Source")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private bool dontDestroyOnLoad = true;

    private void Awake()
    {
        Register();

        if (dontDestroyOnLoad)
            DontDestroyOnLoad(gameObject);
    }

    private void OnValidate()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    public void SetProfile(GameAudioProfile newProfile)
    {
        profile = newProfile;
        Register();
    }

    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        GameAudio.SetMasterVolume(masterVolume);
    }

    public void Play(GameSoundId soundId)
    {
        GameAudio.Play(soundId);
    }

    private void Register()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        GameAudio.Configure(audioSource, profile, masterVolume);
    }
}
