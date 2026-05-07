using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(AudioSource))]
public class AmbientMusicSwitcher : MonoBehaviour
{
    [SerializeField] private string triggerTag = "Player";
    [SerializeField] private bool loopAmbient = true;

    private static AudioSource currentAmbientSource;
    private static AmbientMusicSwitcher currentAmbientSwitcher;
    private static float musicVolume = 1f;

    private AudioSource musicSource;
    private float sourceVolume = 1f;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        currentAmbientSource = null;
        currentAmbientSwitcher = null;
        musicVolume = 1f;
    }

    public static void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);

        if (currentAmbientSwitcher != null)
            currentAmbientSwitcher.ApplyVolume();
    }

    private void Awake()
    {
        musicSource = GetComponent<AudioSource>();
        sourceVolume = musicSource.volume;
        musicSource.loop = loopAmbient;
        ApplyVolume();

        var areaCollider = GetComponent<Collider>();
        areaCollider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        TryActivateAmbient(other);
    }

    private void OnTriggerStay(Collider other)
    {
        if (currentAmbientSource == musicSource && !musicSource.isPlaying)
            TryActivateAmbient(other);
    }

    private void OnDisable()
    {
        if (currentAmbientSource == musicSource)
        {
            currentAmbientSource = null;
            currentAmbientSwitcher = null;
        }
    }

    private void TryActivateAmbient(Collider other)
    {
        if (!string.IsNullOrEmpty(triggerTag) && !other.CompareTag(triggerTag))
        {
            return;
        }

        if (currentAmbientSource == musicSource)
        {
            ApplyVolume();

            if (!musicSource.isPlaying)
                musicSource.Play();

            return;
        }

        if (currentAmbientSource != null && currentAmbientSource.isPlaying)
        {
            currentAmbientSource.Stop();
        }

        if (!musicSource.isPlaying)
        {
            ApplyVolume();
            musicSource.Play();
        }

        currentAmbientSource = musicSource;
        currentAmbientSwitcher = this;
    }

    private void ApplyVolume()
    {
        if (musicSource != null)
            musicSource.volume = sourceVolume * musicVolume;
    }
}
