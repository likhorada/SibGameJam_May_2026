using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public sealed class Room04PassageOpener : MonoBehaviour
{
    [Header("Panel Motions")]
    [SerializeField] private Room04PanelMotion[] panelMotions = new Room04PanelMotion[0];

    [Header("Timing")]
    [SerializeField] private float startDelay = 0.15f;
    [SerializeField] private float panelMoveDuration = 1.25f;
    [SerializeField] private AnimationCurve panelMotionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Passage")]
    [SerializeField] private GameObject cubeHole;
    [SerializeField] private bool hideCubeHoleWhenOpened = true;
    [SerializeField] private Collider[] blockingColliders = new Collider[0];
    [SerializeField] private GameObject[] enableWhenOpened = new GameObject[0];
    [SerializeField] private GameObject[] disableWhenOpened = new GameObject[0];

    [Header("Behavior")]
    [SerializeField] private bool playOnlyOnce = true;
    [SerializeField] private bool useUnscaledTime;
    [SerializeField] private bool logOpening = true;

    [Header("Events")]
    [SerializeField] private UnityEvent onOpeningStarted = new UnityEvent();
    [SerializeField] private UnityEvent onOpened = new UnityEvent();

    private Coroutine openRoutine;
    private MonoBehaviour openRoutineOwner;
    private bool opened;

    public bool IsOpened
    {
        get { return opened; }
    }

    private void OnValidate()
    {
        startDelay = Mathf.Max(0f, startDelay);
        panelMoveDuration = Mathf.Max(0.01f, panelMoveDuration);
        ClampMotionDelays();
    }

    public void Play()
    {
        Open();
    }

    public void Open()
    {
        if (playOnlyOnce && opened)
            return;

        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        StopOpenRoutine();

        openRoutineOwner = GetCoroutineOwner();
        openRoutine = openRoutineOwner.StartCoroutine(OpenRoutine());
    }

    public void OpenInstant()
    {
        if (playOnlyOnce && opened)
            return;

        StopOpenRoutine();

        opened = true;
        onOpeningStarted.Invoke();

        Room04PanelSnapshot[] snapshots = CapturePanelSnapshots();

        for (int i = 0; i < snapshots.Length; i++)
            snapshots[i].Apply(1f);

        CompleteOpening();
    }

    public void ResetOpenedState()
    {
        opened = false;
    }

    private IEnumerator OpenRoutine()
    {
        opened = true;
        onOpeningStarted.Invoke();
        GameAudio.Play(GameSoundId.Activate);

        if (startDelay > 0f)
            yield return Wait(startDelay);

        Room04PanelSnapshot[] snapshots = CapturePanelSnapshots();
        float safeDuration = Mathf.Max(0.0001f, panelMoveDuration);
        float totalDuration = safeDuration + GetMaxDelay(snapshots);
        float elapsed = 0f;
        int activeSnapshotCount = CountActiveSnapshots(snapshots);

        if (logOpening)
            Debug.Log(gameObject.name + ": captured " + activeSnapshotCount + " assigned panel motions");

        if (activeSnapshotCount == 0)
            Debug.LogWarning(gameObject.name + ": Panel Motions has no assigned targets");

        while (elapsed < totalDuration)
        {
            elapsed += DeltaTime();

            for (int i = 0; i < snapshots.Length; i++)
                snapshots[i].Apply(elapsed, safeDuration, panelMotionCurve);

            yield return null;
        }

        for (int i = 0; i < snapshots.Length; i++)
            snapshots[i].Apply(1f);

        CompleteOpening();
        openRoutine = null;
        openRoutineOwner = null;
    }

    private void StopOpenRoutine()
    {
        if (openRoutine == null)
            return;

        if (openRoutineOwner != null)
            openRoutineOwner.StopCoroutine(openRoutine);

        openRoutine = null;
        openRoutineOwner = null;
    }

    private MonoBehaviour GetCoroutineOwner()
    {
        if (isActiveAndEnabled)
            return this;

        return Room04PassageCoroutineHost.Instance;
    }

    private Room04PanelSnapshot[] CapturePanelSnapshots()
    {
        int count = panelMotions == null ? 0 : panelMotions.Length;
        Room04PanelSnapshot[] snapshots = new Room04PanelSnapshot[count];

        for (int i = 0; i < count; i++)
        {
            Room04PanelMotion motion = panelMotions[i];
            Transform panel = motion == null ? null : motion.Target;

            if (panel == null)
            {
                snapshots[i] = Room04PanelSnapshot.Empty;
                continue;
            }

            Quaternion startRotation = panel.localRotation;

            snapshots[i] = new Room04PanelSnapshot(
                panel,
                motion.PositionSpace,
                panel.localPosition,
                panel.localPosition + motion.PositionOffset,
                panel.position,
                panel.position + motion.PositionOffset,
                startRotation,
                startRotation * Quaternion.Euler(motion.EulerOffset),
                motion.Delay
            );
        }

        return snapshots;
    }

    private void ClampMotionDelays()
    {
        if (panelMotions == null)
            return;

        for (int i = 0; i < panelMotions.Length; i++)
        {
            if (panelMotions[i] != null)
                panelMotions[i].Delay = Mathf.Max(0f, panelMotions[i].Delay);
        }
    }

    private static float GetMaxDelay(Room04PanelSnapshot[] snapshots)
    {
        if (snapshots == null)
            return 0f;

        float maxDelay = 0f;

        for (int i = 0; i < snapshots.Length; i++)
            maxDelay = Mathf.Max(maxDelay, snapshots[i].Delay);

        return maxDelay;
    }

    private static int CountActiveSnapshots(Room04PanelSnapshot[] snapshots)
    {
        if (snapshots == null)
            return 0;

        int count = 0;

        for (int i = 0; i < snapshots.Length; i++)
        {
            if (snapshots[i].HasTarget)
                count++;
        }

        return count;
    }

    private void CompleteOpening()
    {
        if (hideCubeHoleWhenOpened && cubeHole != null)
            cubeHole.SetActive(false);

        SetColliderEnabled(blockingColliders, false);
        SetActive(enableWhenOpened, true);
        SetActive(disableWhenOpened, false);
        GameAudio.Play(GameSoundId.DoorOpen);
        onOpened.Invoke();
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

internal sealed class Room04PassageCoroutineHost : MonoBehaviour
{
    private static Room04PassageCoroutineHost instance;

    public static Room04PassageCoroutineHost Instance
    {
        get
        {
            if (instance != null)
                return instance;

            GameObject hostObject = new GameObject("Room04PassageCoroutineHost");
            DontDestroyOnLoad(hostObject);
            instance = hostObject.AddComponent<Room04PassageCoroutineHost>();
            return instance;
        }
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }
}

[Serializable]
public sealed class Room04PanelMotion
{
    [Tooltip("The exact table section transform that should move.")]
    public Transform Target;

    [Tooltip("World keeps X/Y/Z aligned to the scene. Local follows the selected section/parent axes.")]
    public Room04MotionPositionSpace PositionSpace = Room04MotionPositionSpace.World;

    [FormerlySerializedAs("LocalPositionOffset")]
    [Tooltip("Offset from the start position. With Position Space = World, Y is scene up/down.")]
    public Vector3 PositionOffset = new Vector3(0f, -0.75f, 0f);

    [FormerlySerializedAs("LocalEulerOffset")]
    [Tooltip("Extra rotation in degrees. Keep this at 0,0,0 for pure up/down movement.")]
    public Vector3 EulerOffset;

    [Tooltip("Per-section delay before this section starts moving.")]
    public float Delay;

    public Room04PanelMotion()
    {
    }
}

public enum Room04MotionPositionSpace
{
    World = 0,
    Local = 1
}

public readonly struct Room04PanelSnapshot
{
    public static readonly Room04PanelSnapshot Empty =
        new Room04PanelSnapshot(
            null,
            Room04MotionPositionSpace.World,
            Vector3.zero,
            Vector3.zero,
            Vector3.zero,
            Vector3.zero,
            Quaternion.identity,
            Quaternion.identity,
            0f);

    private readonly Transform panel;
    private readonly Room04MotionPositionSpace positionSpace;
    private readonly Vector3 startPosition;
    private readonly Vector3 endPosition;
    private readonly Vector3 startWorldPosition;
    private readonly Vector3 endWorldPosition;
    private readonly Quaternion startRotation;
    private readonly Quaternion endRotation;
    private readonly float delay;

    public float Delay
    {
        get { return delay; }
    }

    public bool HasTarget
    {
        get { return panel != null; }
    }

    public Room04PanelSnapshot(
        Transform panel,
        Room04MotionPositionSpace positionSpace,
        Vector3 startPosition,
        Vector3 endPosition,
        Vector3 startWorldPosition,
        Vector3 endWorldPosition,
        Quaternion startRotation,
        Quaternion endRotation,
        float delay)
    {
        this.panel = panel;
        this.positionSpace = positionSpace;
        this.startPosition = startPosition;
        this.endPosition = endPosition;
        this.startWorldPosition = startWorldPosition;
        this.endWorldPosition = endWorldPosition;
        this.startRotation = startRotation;
        this.endRotation = endRotation;
        this.delay = Mathf.Max(0f, delay);
    }

    public void Apply(float t)
    {
        if (panel == null)
            return;

        if (positionSpace == Room04MotionPositionSpace.World)
            panel.position = Vector3.LerpUnclamped(startWorldPosition, endWorldPosition, t);
        else
            panel.localPosition = Vector3.LerpUnclamped(startPosition, endPosition, t);

        panel.localRotation = Quaternion.SlerpUnclamped(startRotation, endRotation, t);
    }

    public void Apply(float elapsed, float duration, AnimationCurve curve)
    {
        if (elapsed < delay)
        {
            Apply(0f);
            return;
        }

        float normalizedTime = Mathf.Clamp01((elapsed - delay) / Mathf.Max(0.0001f, duration));
        float easedTime = curve == null ? normalizedTime : curve.Evaluate(normalizedTime);
        Apply(easedTime);
    }
}
