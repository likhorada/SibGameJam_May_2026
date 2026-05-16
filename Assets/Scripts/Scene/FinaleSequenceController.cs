using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.Video;

public sealed class FinaleSequenceController : MonoBehaviour
{
    [Header("Events")]
    [SerializeField] private UnityEvent onSequenceStarted = new UnityEvent();
    [SerializeField] private UnityEvent onBeforeEndingWindow = new UnityEvent();
    [SerializeField] private UnityEvent onSequenceFinished = new UnityEvent();

    [Header("Animator")]
    [SerializeField] private FinaleAnimatorTrigger[] animatorTriggers = new FinaleAnimatorTrigger[0];

    [Header("Timeline")]
    [SerializeField] private PlayableDirector timeline;
    [SerializeField] private bool waitForTimeline = true;

    [Header("Video")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private GameObject videoRoot;
    [SerializeField] private bool showVideoRootWhilePlaying = true;
    [SerializeField] private bool waitForVideo = true;

    [Header("Ending")]
    [SerializeField] private FinalEndingWindowController endingWindow;
    [SerializeField] private float delayBeforeEndingWindow;
    [SerializeField] private bool useUnscaledTime;
    [SerializeField] private bool playOnlyOnce = true;

    private Coroutine sequenceRoutine;
    private bool hasPlayed;

    public bool IsPlaying
    {
        get { return sequenceRoutine != null; }
    }

    public void Play()
    {
        if (playOnlyOnce && hasPlayed)
            return;

        if (sequenceRoutine != null)
            StopCoroutine(sequenceRoutine);

        sequenceRoutine = StartCoroutine(PlayRoutine());
    }

    public void ResetPlayedState()
    {
        hasPlayed = false;
    }

    private IEnumerator PlayRoutine()
    {
        hasPlayed = true;
        onSequenceStarted.Invoke();
        FireAnimatorTriggers();

        if (timeline != null)
        {
            timeline.Play();

            if (waitForTimeline)
                yield return WaitForTimeline();
        }

        if (videoPlayer != null && HasVideoContent(videoPlayer))
        {
            if (videoRoot != null && showVideoRootWhilePlaying)
                videoRoot.SetActive(true);

            videoPlayer.Play();

            if (waitForVideo)
                yield return WaitForVideo();

            if (videoRoot != null && showVideoRootWhilePlaying)
                videoRoot.SetActive(false);
        }

        if (delayBeforeEndingWindow > 0f)
            yield return Wait(delayBeforeEndingWindow);

        onBeforeEndingWindow.Invoke();

        if (endingWindow != null)
            endingWindow.ShowEnding();

        onSequenceFinished.Invoke();
        sequenceRoutine = null;
    }

    private void FireAnimatorTriggers()
    {
        if (animatorTriggers == null)
            return;

        for (int i = 0; i < animatorTriggers.Length; i++)
            animatorTriggers[i].Fire();
    }

    private IEnumerator WaitForTimeline()
    {
        yield return null;

        while (timeline != null && timeline.state == PlayState.Playing)
            yield return null;
    }

    private IEnumerator WaitForVideo()
    {
        yield return null;

        float timeout = videoPlayer.length > 0.1 ? (float)videoPlayer.length + 1f : 300f;
        float elapsed = 0f;

        while (videoPlayer != null && videoPlayer.isPlaying && elapsed < timeout)
        {
            elapsed += DeltaTime();
            yield return null;
        }
    }

    private IEnumerator Wait(float seconds)
    {
        if (useUnscaledTime)
            yield return new WaitForSecondsRealtime(seconds);
        else
            yield return new WaitForSeconds(seconds);
    }

    private float DeltaTime()
    {
        return useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
    }

    private static bool HasVideoContent(VideoPlayer player)
    {
        if (player == null)
            return false;

        return player.clip != null || !string.IsNullOrEmpty(player.url);
    }
}

[System.Serializable]
public sealed class FinaleAnimatorTrigger
{
    public Animator Animator;
    public string TriggerName;

    public void Fire()
    {
        if (Animator == null || string.IsNullOrEmpty(TriggerName))
            return;

        Animator.SetTrigger(TriggerName);
    }
}
