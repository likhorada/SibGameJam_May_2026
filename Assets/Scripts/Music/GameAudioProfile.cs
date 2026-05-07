using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Golem Craft/Audio/Game Audio Profile")]
public sealed class GameAudioProfile : ScriptableObject
{
    [SerializeField] private GameAudioEntry[] sounds = new GameAudioEntry[0];

    public bool TryGetSound(GameSoundId soundId, out AudioClip clip, out float volume, out float pitch)
    {
        clip = null;
        volume = 1f;
        pitch = 1f;

        if (sounds == null)
            return false;

        for (int i = 0; i < sounds.Length; i++)
        {
            GameAudioEntry entry = sounds[i];

            if (entry == null || entry.SoundId != soundId || entry.Clip == null)
                continue;

            clip = entry.Clip;
            volume = Mathf.Clamp01(entry.Volume);
            pitch = entry.GetPitch();
            return true;
        }

        return false;
    }
}

[Serializable]
public sealed class GameAudioEntry
{
    [SerializeField] private GameSoundId soundId;
    [SerializeField] private AudioClip clip;
    [SerializeField, Range(0f, 1f)] private float volume = 0.75f;
    [SerializeField] private Vector2 pitchRange = Vector2.one;

    public GameSoundId SoundId
    {
        get { return soundId; }
    }

    public AudioClip Clip
    {
        get { return clip; }
    }

    public float Volume
    {
        get { return volume; }
    }

    public float GetPitch()
    {
        float min = Mathf.Max(0.01f, Mathf.Min(pitchRange.x, pitchRange.y));
        float max = Mathf.Max(0.01f, Mathf.Max(pitchRange.x, pitchRange.y));

        if (Mathf.Approximately(min, max))
            return min;

        return UnityEngine.Random.Range(min, max);
    }
}
