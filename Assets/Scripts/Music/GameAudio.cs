using UnityEngine;

public enum GameSoundId
{
    UiClick,
    UiDrag,
    UiDrop,
    TableOpen,
    TableClose,
    CraftSuccess,
    CraftFail,
    CollectElement,
    InventoryFull,
    Activate,
    OfferingPlace,
    OfferingComplete,
    DoorOpen,
    Locked,
    Trash,
    GameOver
}

public static class GameAudio
{
    private const int SampleRate = 44100;

    private static AudioSource source;
    private static GameAudioProfile profile;
    private static float masterVolume = 1f;
    private static AudioClip uiClick;
    private static AudioClip uiDrag;
    private static AudioClip uiDrop;
    private static AudioClip tableOpen;
    private static AudioClip tableClose;
    private static AudioClip craftSuccess;
    private static AudioClip craftFail;
    private static AudioClip collectElement;
    private static AudioClip inventoryFull;
    private static AudioClip activate;
    private static AudioClip offeringPlace;
    private static AudioClip offeringComplete;
    private static AudioClip doorOpen;
    private static AudioClip locked;
    private static AudioClip trash;
    private static AudioClip gameOver;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        source = null;
        profile = null;
        masterVolume = 1f;
        uiClick = null;
        uiDrag = null;
        uiDrop = null;
        tableOpen = null;
        tableClose = null;
        craftSuccess = null;
        craftFail = null;
        collectElement = null;
        inventoryFull = null;
        activate = null;
        offeringPlace = null;
        offeringComplete = null;
        doorOpen = null;
        locked = null;
        trash = null;
        gameOver = null;
    }

    public static void Play(GameSoundId soundId)
    {
        EnsureSource();

        if (profile != null
            && profile.TryGetSound(soundId, out AudioClip customClip, out float customVolume, out float customPitch))
        {
            PlayClip(customClip, customVolume, customPitch);
            return;
        }

        AudioClip clip = GetClip(soundId);

        if (clip == null)
            return;

        PlayClip(clip, GetVolume(soundId), 1f);
    }

    public static void PlayUiClick()
    {
        Play(GameSoundId.UiClick);
    }

    public static void Configure(AudioSource audioSource, GameAudioProfile audioProfile, float volume)
    {
        if (audioSource != null)
            source = audioSource;

        profile = audioProfile;
        masterVolume = Mathf.Clamp01(volume);

        if (source != null)
            ConfigureSource(source);
    }

    public static void SetProfile(GameAudioProfile audioProfile)
    {
        profile = audioProfile;
    }

    public static void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
    }

    private static void EnsureSource()
    {
        if (source != null)
            return;

        GameObject audioObject = new GameObject("GameAudio");
        UnityEngine.Object.DontDestroyOnLoad(audioObject);

        source = audioObject.AddComponent<AudioSource>();
        ConfigureSource(source);
    }

    private static void ConfigureSource(AudioSource audioSource)
    {
        if (audioSource == null)
            return;

        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        audioSource.volume = 1f;
    }

    private static void PlayClip(AudioClip clip, float volume, float pitch)
    {
        if (clip == null || source == null)
            return;

        float previousPitch = source.pitch;

        source.pitch = Mathf.Approximately(pitch, 0f) ? 1f : pitch;
        source.PlayOneShot(clip, Mathf.Clamp01(volume) * masterVolume);
        source.pitch = previousPitch;
    }

    private static AudioClip GetClip(GameSoundId soundId)
    {
        switch (soundId)
        {
            case GameSoundId.UiClick:
                return uiClick ?? (uiClick = CreateTone("SFX_UI_Click", 920f, 0.06f, 0.26f, -1800f));
            case GameSoundId.UiDrag:
                return uiDrag ?? (uiDrag = CreateTone("SFX_UI_Drag", 520f, 0.07f, 0.16f, 420f));
            case GameSoundId.UiDrop:
                return uiDrop ?? (uiDrop = CreateTone("SFX_UI_Drop", 260f, 0.08f, 0.22f, -260f));
            case GameSoundId.TableOpen:
                return tableOpen ?? (tableOpen = CreateTone("SFX_Table_Open", 180f, 0.18f, 0.28f, 120f));
            case GameSoundId.TableClose:
                return tableClose ?? (tableClose = CreateTone("SFX_Table_Close", 150f, 0.12f, 0.24f, -90f));
            case GameSoundId.CraftSuccess:
                return craftSuccess ?? (craftSuccess = CreateArpeggio("SFX_Craft_Success", 392f, 0.28f, 0.25f, 1.5f));
            case GameSoundId.CraftFail:
                return craftFail ?? (craftFail = CreateTone("SFX_Craft_Fail", 170f, 0.18f, 0.25f, -180f));
            case GameSoundId.CollectElement:
                return collectElement ?? (collectElement = CreateArpeggio("SFX_Collect_Element", 523.25f, 0.18f, 0.2f, 1.25f));
            case GameSoundId.InventoryFull:
                return inventoryFull ?? (inventoryFull = CreateTone("SFX_Inventory_Full", 110f, 0.16f, 0.25f, -30f));
            case GameSoundId.Activate:
                return activate ?? (activate = CreateArpeggio("SFX_Activate", 261.63f, 0.36f, 0.28f, 2f));
            case GameSoundId.OfferingPlace:
                return offeringPlace ?? (offeringPlace = CreateNoiseHit("SFX_Offering_Place", 0.16f, 0.26f, 380f));
            case GameSoundId.OfferingComplete:
                return offeringComplete ?? (offeringComplete = CreateArpeggio("SFX_Offering_Complete", 329.63f, 0.5f, 0.32f, 2f));
            case GameSoundId.DoorOpen:
                return doorOpen ?? (doorOpen = CreateNoiseHit("SFX_Door_Open", 0.3f, 0.3f, 120f));
            case GameSoundId.Locked:
                return locked ?? (locked = CreateTone("SFX_Locked", 95f, 0.11f, 0.3f, -20f));
            case GameSoundId.Trash:
                return trash ?? (trash = CreateNoiseHit("SFX_Trash", 0.24f, 0.28f, 260f));
            case GameSoundId.GameOver:
                return gameOver ?? (gameOver = CreateTone("SFX_Game_Over", 82f, 0.6f, 0.36f, -48f));
            default:
                return null;
        }
    }

    private static float GetVolume(GameSoundId soundId)
    {
        switch (soundId)
        {
            case GameSoundId.UiClick:
            case GameSoundId.UiDrag:
            case GameSoundId.UiDrop:
                return 0.65f;
            case GameSoundId.GameOver:
                return 0.85f;
            default:
                return 0.75f;
        }
    }

    private static AudioClip CreateTone(
        string name,
        float startFrequency,
        float duration,
        float amplitude,
        float frequencyBend)
    {
        int sampleCount = Mathf.Max(1, Mathf.RoundToInt(duration * SampleRate));
        float[] data = new float[sampleCount];
        float phase = 0f;

        for (int i = 0; i < sampleCount; i++)
        {
            float t = i / (float)sampleCount;
            float frequency = Mathf.Max(20f, startFrequency + frequencyBend * t);
            phase += frequency / SampleRate;

            float envelope = Envelope(t);
            float sample = Mathf.Sin(phase * Mathf.PI * 2f);
            data[i] = sample * envelope * amplitude;
        }

        return CreateClip(name, data);
    }

    private static AudioClip CreateArpeggio(
        string name,
        float baseFrequency,
        float duration,
        float amplitude,
        float finalMultiplier)
    {
        int sampleCount = Mathf.Max(1, Mathf.RoundToInt(duration * SampleRate));
        float[] data = new float[sampleCount];
        float phase = 0f;

        for (int i = 0; i < sampleCount; i++)
        {
            float t = i / (float)sampleCount;
            float step = t < 0.33f ? 1f : t < 0.66f ? 1.25f : finalMultiplier;
            phase += baseFrequency * step / SampleRate;

            float envelope = Envelope(t);
            float sample = Mathf.Sin(phase * Mathf.PI * 2f);
            data[i] = sample * envelope * amplitude;
        }

        return CreateClip(name, data);
    }

    private static AudioClip CreateNoiseHit(
        string name,
        float duration,
        float amplitude,
        float toneFrequency)
    {
        int sampleCount = Mathf.Max(1, Mathf.RoundToInt(duration * SampleRate));
        float[] data = new float[sampleCount];
        float phase = 0f;
        uint state = 2166136261u;

        for (int i = 0; i < sampleCount; i++)
        {
            float t = i / (float)sampleCount;
            unchecked
            {
                state = state * 16777619u + 1013904223u;
            }

            float noise = ((state & 0xffff) / 32768f) - 1f;
            phase += toneFrequency / SampleRate;

            float tone = Mathf.Sin(phase * Mathf.PI * 2f) * 0.45f;
            float envelope = Envelope(t);
            data[i] = (noise * 0.55f + tone) * envelope * amplitude;
        }

        return CreateClip(name, data);
    }

    private static AudioClip CreateClip(string name, float[] data)
    {
        AudioClip clip = AudioClip.Create(name, data.Length, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }

    private static float Envelope(float t)
    {
        float attack = Mathf.Clamp01(t / 0.08f);
        float release = Mathf.Clamp01((1f - t) / 0.35f);
        return Mathf.SmoothStep(0f, 1f, attack) * Mathf.SmoothStep(0f, 1f, release);
    }
}
