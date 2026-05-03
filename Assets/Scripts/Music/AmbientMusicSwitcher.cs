using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(AudioSource))]
public class AmbientMusicSwitcher : MonoBehaviour
{
    [SerializeField] private string triggerTag = "Player";

    private static AudioSource currentAmbientSource;
    private AudioSource musicSource;

    private void Awake()
    {
        musicSource = GetComponent<AudioSource>();

        var areaCollider = GetComponent<Collider>();
        areaCollider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!string.IsNullOrEmpty(triggerTag) && !other.CompareTag(triggerTag))
        {
            return;
        }

        if (currentAmbientSource == musicSource)
        {
            return;
        }

        if (currentAmbientSource != null && currentAmbientSource.isPlaying)
        {
            currentAmbientSource.Stop();
        }

        if (!musicSource.isPlaying)
        {
            musicSource.Play();
        }

        currentAmbientSource = musicSource;
    }
}
