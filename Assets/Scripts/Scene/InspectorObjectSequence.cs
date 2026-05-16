using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Inspector-driven helper for simple scene reveals: move objects and toggle GameObjects/colliders.
/// </summary>
public sealed class InspectorObjectSequence : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField] private float startDelay;
    [SerializeField] private float duration = 1f;
    [SerializeField] private AnimationCurve motionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private bool useUnscaledTime;
    [SerializeField] private bool playOnlyOnce = true;

    [Header("Transforms")]
    [SerializeField] private InspectorTransformTarget[] transformTargets = new InspectorTransformTarget[0];

    [Header("At Start")]
    [SerializeField] private GameObject[] activateOnStart = new GameObject[0];
    [SerializeField] private GameObject[] deactivateOnStart = new GameObject[0];
    [SerializeField] private Collider[] enableCollidersOnStart = new Collider[0];
    [SerializeField] private Collider[] disableCollidersOnStart = new Collider[0];

    [Header("At Complete")]
    [SerializeField] private GameObject[] activateOnComplete = new GameObject[0];
    [SerializeField] private GameObject[] deactivateOnComplete = new GameObject[0];
    [SerializeField] private Collider[] enableCollidersOnComplete = new Collider[0];
    [SerializeField] private Collider[] disableCollidersOnComplete = new Collider[0];

    [Header("Events")]
    [SerializeField] private UnityEvent onBeforePlay = new UnityEvent();
    [SerializeField] private UnityEvent onAfterPlay = new UnityEvent();

    private Coroutine playRoutine;
    private bool hasPlayed;

    public bool IsPlaying
    {
        get { return playRoutine != null; }
    }

    public bool HasPlayed
    {
        get { return hasPlayed; }
    }

    public void Play()
    {
        if (playOnlyOnce && hasPlayed)
            return;

        if (playRoutine != null)
            StopCoroutine(playRoutine);

        playRoutine = StartCoroutine(PlayRoutine());
    }

    public void PlayInstant()
    {
        if (playOnlyOnce && hasPlayed)
            return;

        if (playRoutine != null)
        {
            StopCoroutine(playRoutine);
            playRoutine = null;
        }

        onBeforePlay.Invoke();
        ApplyStartToggles();

        for (int i = 0; i < transformTargets.Length; i++)
        {
            if (transformTargets[i] != null)
                transformTargets[i].ApplyInstant();
        }

        ApplyCompleteToggles();
        hasPlayed = true;
        onAfterPlay.Invoke();
    }

    public void ResetPlayedState()
    {
        hasPlayed = false;
    }

    private IEnumerator PlayRoutine()
    {
        hasPlayed = true;
        onBeforePlay.Invoke();
        ApplyStartToggles();

        if (startDelay > 0f)
            yield return Wait(startDelay);

        InspectorTransformSnapshot[] snapshots = CaptureSnapshots();
        float safeDuration = Mathf.Max(0.0001f, duration);
        float elapsed = 0f;

        while (elapsed < safeDuration)
        {
            elapsed += DeltaTime();
            float normalizedTime = Mathf.Clamp01(elapsed / safeDuration);
            float easedTime = motionCurve == null ? normalizedTime : motionCurve.Evaluate(normalizedTime);
            ApplySnapshots(snapshots, easedTime);
            yield return null;
        }

        ApplySnapshots(snapshots, 1f);
        ApplyCompleteToggles();
        onAfterPlay.Invoke();
        playRoutine = null;
    }

    private InspectorTransformSnapshot[] CaptureSnapshots()
    {
        int count = transformTargets == null ? 0 : transformTargets.Length;
        InspectorTransformSnapshot[] snapshots = new InspectorTransformSnapshot[count];

        for (int i = 0; i < count; i++)
        {
            snapshots[i] = transformTargets[i] == null
                ? InspectorTransformSnapshot.Empty
                : transformTargets[i].Capture();
        }

        return snapshots;
    }

    private static void ApplySnapshots(InspectorTransformSnapshot[] snapshots, float t)
    {
        for (int i = 0; i < snapshots.Length; i++)
            snapshots[i].Apply(t);
    }

    private void ApplyStartToggles()
    {
        SetActive(activateOnStart, true);
        SetActive(deactivateOnStart, false);
        SetColliderEnabled(enableCollidersOnStart, true);
        SetColliderEnabled(disableCollidersOnStart, false);
    }

    private void ApplyCompleteToggles()
    {
        SetActive(activateOnComplete, true);
        SetActive(deactivateOnComplete, false);
        SetColliderEnabled(enableCollidersOnComplete, true);
        SetColliderEnabled(disableCollidersOnComplete, false);
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

    private static void SetActive(GameObject[] targets, bool active)
    {
        if (targets == null)
            return;

        for (int i = 0; i < targets.Length; i++)
        {
            if (targets[i] != null)
                targets[i].SetActive(active);
        }
    }

    private static void SetColliderEnabled(Collider[] colliders, bool enabled)
    {
        if (colliders == null)
            return;

        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i] != null)
                colliders[i].enabled = enabled;
        }
    }
}

[Serializable]
public sealed class InspectorTransformTarget
{
    public Transform Target;
    public Vector3 LocalPositionOffset;
    public Vector3 LocalEulerOffset;

    public InspectorTransformSnapshot Capture()
    {
        if (Target == null)
            return InspectorTransformSnapshot.Empty;

        Quaternion startRotation = Target.localRotation;

        return new InspectorTransformSnapshot(
            Target,
            Target.localPosition,
            Target.localPosition + LocalPositionOffset,
            startRotation,
            startRotation * Quaternion.Euler(LocalEulerOffset)
        );
    }

    public void ApplyInstant()
    {
        Capture().Apply(1f);
    }
}

public readonly struct InspectorTransformSnapshot
{
    public static readonly InspectorTransformSnapshot Empty =
        new InspectorTransformSnapshot(null, Vector3.zero, Vector3.zero, Quaternion.identity, Quaternion.identity);

    private readonly Transform target;
    private readonly Vector3 startPosition;
    private readonly Vector3 endPosition;
    private readonly Quaternion startRotation;
    private readonly Quaternion endRotation;

    public InspectorTransformSnapshot(
        Transform target,
        Vector3 startPosition,
        Vector3 endPosition,
        Quaternion startRotation,
        Quaternion endRotation)
    {
        this.target = target;
        this.startPosition = startPosition;
        this.endPosition = endPosition;
        this.startRotation = startRotation;
        this.endRotation = endRotation;
    }

    public void Apply(float t)
    {
        if (target == null)
            return;

        target.localPosition = Vector3.LerpUnclamped(startPosition, endPosition, t);
        target.localRotation = Quaternion.SlerpUnclamped(startRotation, endRotation, t);
    }
}
