using System;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public sealed class PlayerEmbeddedClipAnimator : MonoBehaviour
{
    [Header("Runtime Target")]
    [SerializeField] private Transform visualRoot;
    [HideInInspector] [SerializeField] private bool waitForConfiguredVisual = true;

    [Header("Animation")]
    [SerializeField] private PlaybackBackend playbackBackend = PlaybackBackend.LegacyAnimationComponent;
    [SerializeField] private string forceActionId = "";

    [HideInInspector] [SerializeField] private string modelResourcePath;
    [HideInInspector] [SerializeField] private string fallbackModelResourcePath;

    [HideInInspector] [SerializeField] private string idleActionId = "idle";
    [HideInInspector] [SerializeField] private string walkActionId = "walk";
    [HideInInspector] [SerializeField] private string idleExactClipName = "Armature|IDLE";
    [HideInInspector] [SerializeField] private string walkExactClipName = "Armature|WALK";
    [HideInInspector] [SerializeField] private string idleNameContains = "idle";
    [HideInInspector] [SerializeField] private string walkNameContains = "walk";

    [Header("Additional Actions")]
    [SerializeField] private EmbeddedClipAction[] extraActions = new EmbeddedClipAction[0];

    [Header("Locomotion")]
    [SerializeField] private float walkThreshold = 0.05f;
    [SerializeField] private float blendSpeed = 8f;
    [SerializeField] private float walkSwingDegrees = 28f;
    [SerializeField] private float walkBobAmplitude = 0.045f;
    [SerializeField] private float walkFrequency = 6.5f;
    [SerializeField] private float idleBobAmplitude = 0.012f;
    [SerializeField] private float idleFrequency = 1.5f;

    [HideInInspector] [SerializeField] private float idlePlaybackSpeed = 0.35f;
    [HideInInspector] [SerializeField] private float walkPlaybackSpeed = 1f;
    [HideInInspector] [SerializeField] private bool useTransformDeltaSpeed = true;

    [Header("Diagnostics")]
    [SerializeField] private bool logLoadedClips = true;
    [SerializeField] private string currentActionId = "";
    [SerializeField] private float currentPlanarSpeed;
    [HideInInspector] [SerializeField] private bool logMissingConfiguredActions = true;
    [HideInInspector] [SerializeField] private bool logActionChanges = true;

    private Rigidbody parentRigidbody;
    private Animator animator;
    private Animation legacyAnimation;
    private Avatar loadedAvatar;
    private AnimationClip[] clips = new AnimationClip[0];
    private RuntimeClipAction[] runtimeActions = new RuntimeClipAction[0];
    private PlayableGraph graph;
    private AnimationMixerPlayable mixer;
    private int idleActionIndex = -1;
    private int walkActionIndex = -1;
    private int forcedActionIndex = -1;
    private bool initialized;
    private Vector3 previousPosition;
    private int lastLoggedActionIndex = -1;
    private int currentLegacyActionIndex = -1;
    private BodyPartPose[] bodyPartPoses = new BodyPartPose[0];
    private float locomotionBlend;
    private float motionPhase;

    private enum PlaybackBackend
    {
        ProceduralBodyMotion,
        LegacyAnimationComponent,
        PlayableGraph
    }

    [Serializable]
    private sealed class EmbeddedClipAction
    {
        public string actionId = "";
        public string exactClipName = "";
        public string nameContains = "";
        public float playbackSpeed = 1f;
        public bool loop = true;
    }

    private struct RuntimeClipAction
    {
        public string ActionId;
        public AnimationClip Clip;
        public string LegacyClipName;
        public AnimationClipPlayable Playable;
        public float PlaybackSpeed;
        public bool Loop;
        public float Weight;
    }

    private struct BodyPartPose
    {
        public Transform Transform;
        public Vector3 BaseLocalPosition;
        public Quaternion BaseLocalRotation;
        public BodyPartRole Role;
    }

    private enum BodyPartRole
    {
        Body,
        LeftArm,
        RightArm,
        LeftLeg,
        RightLeg
    }

    public void Configure(string resourcePath, string fallbackResourcePath)
    {
        modelResourcePath = resourcePath;
        fallbackModelResourcePath = fallbackResourcePath;
        TryInitialize();
    }

    public void Configure(Transform loadedVisualRoot, string resourcePath, string fallbackResourcePath)
    {
        visualRoot = loadedVisualRoot;
        modelResourcePath = resourcePath;
        fallbackModelResourcePath = fallbackResourcePath;
        TryInitialize();
    }

    public bool PlayAction(string actionId)
    {
        if (!initialized)
            TryInitialize();

        int index = FindActionIndex(actionId);

        if (index < 0)
            return false;

        forcedActionIndex = index;
        return true;
    }

    public void ClearActionOverride()
    {
        forcedActionIndex = -1;
    }

    private void Awake()
    {
        parentRigidbody = GetComponent<Rigidbody>();

        if (parentRigidbody == null)
            parentRigidbody = GetComponentInParent<Rigidbody>();

        TryInitialize();
    }

    private void OnEnable()
    {
        if (initialized && graph.IsValid())
            graph.Play();
    }

    private void Update()
    {
        if (!initialized)
            TryInitialize();

        if (!initialized)
            return;

        UpdateWeights();
        UpdatePlayback();
    }

    private void OnDisable()
    {
        if (graph.IsValid())
            graph.Stop();
    }

    private void OnDestroy()
    {
        if (graph.IsValid())
            graph.Destroy();
    }

    private void TryInitialize()
    {
        if (initialized || string.IsNullOrWhiteSpace(modelResourcePath))
            return;

        if (waitForConfiguredVisual && visualRoot == null)
            return;

        clips = LoadClips(modelResourcePath);
        loadedAvatar = LoadAvatar(modelResourcePath);

        if (clips.Length == 0 && !string.IsNullOrWhiteSpace(fallbackModelResourcePath))
            clips = LoadClips(fallbackModelResourcePath);

        if (loadedAvatar == null && !string.IsNullOrWhiteSpace(fallbackModelResourcePath))
            loadedAvatar = LoadAvatar(fallbackModelResourcePath);

        Transform searchRoot = visualRoot == null ? transform : visualRoot;
        CacheBodyParts(searchRoot);

        if (playbackBackend != PlaybackBackend.ProceduralBodyMotion)
        {
            if (clips.Length == 0)
                return;

            animator = FindAnimator(searchRoot);
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            animator.applyRootMotion = false;
            animator.enabled = playbackBackend == PlaybackBackend.PlayableGraph;

            if (animator.avatar == null && loadedAvatar != null)
                animator.avatar = loadedAvatar;

            if (animator.avatar == null)
                Debug.LogWarning(gameObject.name + ": Animator has no Avatar. Set the FBX Rig to Generic and Avatar Definition to Create From This Model, then reimport.");
            else if (!animator.avatar.isValid)
                Debug.LogWarning(gameObject.name + ": Animator Avatar is invalid. Reimport the FBX and verify the Rig tab shows a valid Generic avatar.");
        }

        BuildRuntimeActions();

        if (runtimeActions.Length == 0)
            AddBuiltInProceduralActions();

        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
        }

        previousPosition = transform.position;

        if (playbackBackend == PlaybackBackend.LegacyAnimationComponent)
            BuildLegacyAnimation(searchRoot);
        else
        {
            if (playbackBackend == PlaybackBackend.PlayableGraph)
                BuildGraph();
        }

        initialized = true;

        if (logLoadedClips)
            Debug.Log(BuildLoadedClipLog());
    }

    private AnimationClip[] LoadClips(string resourcePath)
    {
        UnityEngine.Object[] loadedAssets = Resources.LoadAll(resourcePath);
        int clipCount = 0;

        for (int i = 0; i < loadedAssets.Length; i++)
        {
            AnimationClip clip = loadedAssets[i] as AnimationClip;

            if (IsUsableClip(clip))
                clipCount++;
        }

        AnimationClip[] result = new AnimationClip[clipCount];
        int nextClip = 0;

        for (int i = 0; i < loadedAssets.Length; i++)
        {
            AnimationClip clip = loadedAssets[i] as AnimationClip;

            if (IsUsableClip(clip))
                result[nextClip++] = clip;
        }

        return result;
    }

    private Avatar LoadAvatar(string resourcePath)
    {
        UnityEngine.Object[] loadedAssets = Resources.LoadAll(resourcePath);
        Avatar firstAvatar = null;

        for (int i = 0; i < loadedAssets.Length; i++)
        {
            Avatar avatar = loadedAssets[i] as Avatar;

            if (avatar == null)
                continue;

            if (avatar.isValid)
                return avatar;

            if (firstAvatar == null)
                firstAvatar = avatar;
        }

        return firstAvatar;
    }

    private Animator FindAnimator(Transform searchRoot)
    {
        Animator[] animators = searchRoot.GetComponentsInChildren<Animator>(true);

        for (int i = 0; i < animators.Length; i++)
        {
            if (animators[i] != null && animators[i].avatar != null)
                return animators[i];
        }

        if (animators.Length > 0 && animators[0] != null)
            return animators[0];

        return searchRoot.gameObject.AddComponent<Animator>();
    }

    private void BuildRuntimeActions()
    {
        runtimeActions = new RuntimeClipAction[0];

        if (playbackBackend != PlaybackBackend.ProceduralBodyMotion)
        {
            idleActionIndex = AddAction(idleActionId, idleExactClipName, idleNameContains, idlePlaybackSpeed, true);
            walkActionIndex = AddAction(walkActionId, walkExactClipName, walkNameContains, walkPlaybackSpeed, true);
        }

        for (int i = 0; i < extraActions.Length; i++)
        {
            EmbeddedClipAction action = extraActions[i];

            if (action == null || string.IsNullOrWhiteSpace(action.actionId))
                continue;

            AddAction(action.actionId, action.exactClipName, action.nameContains, action.playbackSpeed, action.loop);
        }

        if (idleActionIndex < 0 && runtimeActions.Length > 0)
            idleActionIndex = 0;

        if (walkActionIndex < 0)
            walkActionIndex = idleActionIndex;
    }

    private void AddBuiltInProceduralActions()
    {
        idleActionIndex = AddProceduralAction(idleActionId, idlePlaybackSpeed, true);
        walkActionIndex = AddProceduralAction(walkActionId, walkPlaybackSpeed, true);
    }

    private int AddProceduralAction(string actionId, float playbackSpeed, bool loop)
    {
        int index = runtimeActions.Length;
        Array.Resize(ref runtimeActions, runtimeActions.Length + 1);
        runtimeActions[index] = new RuntimeClipAction
        {
            ActionId = actionId,
            Clip = null,
            LegacyClipName = actionId,
            PlaybackSpeed = playbackSpeed,
            Loop = loop,
            Weight = index == 0 ? 1f : 0f
        };

        return index;
    }

    private int AddAction(string actionId, string exactClipName, string nameContains, float playbackSpeed, bool loop)
    {
        AnimationClip clip = ChooseClip(clips, exactClipName, nameContains);

        if (clip == null)
        {
            if (logMissingConfiguredActions)
                Debug.LogWarning(gameObject.name + ": no embedded clip found for action '" + actionId + "'. Exact='" + exactClipName + "', contains='" + nameContains + "'.");

            return -1;
        }

        int index = runtimeActions.Length;
        Array.Resize(ref runtimeActions, runtimeActions.Length + 1);
        runtimeActions[index] = new RuntimeClipAction
        {
            ActionId = actionId,
            Clip = clip,
            LegacyClipName = actionId,
            PlaybackSpeed = playbackSpeed,
            Loop = loop,
            Weight = index == 0 ? 1f : 0f
        };

        return index;
    }

    private void BuildGraph()
    {
        if (graph.IsValid())
            graph.Destroy();

        graph = PlayableGraph.Create("PlayerEmbeddedClipAnimator");
        graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);

        mixer = AnimationMixerPlayable.Create(graph, runtimeActions.Length);

        for (int i = 0; i < runtimeActions.Length; i++)
        {
            RuntimeClipAction action = runtimeActions[i];
            action.Playable = AnimationClipPlayable.Create(graph, action.Clip);
            action.Playable.SetApplyFootIK(false);
            action.Playable.SetSpeed(action.PlaybackSpeed);
            runtimeActions[i] = action;

            graph.Connect(action.Playable, 0, mixer, i);
            mixer.SetInputWeight(i, action.Weight);
        }

        AnimationPlayableOutput output = AnimationPlayableOutput.Create(graph, "Animation", animator);
        output.SetSourcePlayable(mixer);

        graph.Play();
    }

    private void BuildLegacyAnimation(Transform searchRoot)
    {
        if (graph.IsValid())
            graph.Destroy();

        legacyAnimation = searchRoot.GetComponent<Animation>();

        if (legacyAnimation == null)
            legacyAnimation = searchRoot.gameObject.AddComponent<Animation>();

        legacyAnimation.playAutomatically = false;
        legacyAnimation.cullingType = AnimationCullingType.AlwaysAnimate;

        for (int i = 0; i < runtimeActions.Length; i++)
        {
            RuntimeClipAction action = runtimeActions[i];
            AnimationClip legacyClip = Instantiate(action.Clip);
            legacyClip.name = action.ActionId;
            legacyClip.legacy = true;
            legacyClip.wrapMode = action.Loop ? WrapMode.Loop : WrapMode.Once;

            action.Clip = legacyClip;
            action.LegacyClipName = legacyClip.name;
            runtimeActions[i] = action;

            legacyAnimation.AddClip(legacyClip, action.LegacyClipName);

            AnimationState state = legacyAnimation[action.LegacyClipName];

            if (state != null)
            {
                state.speed = action.PlaybackSpeed;
                state.wrapMode = action.Loop ? WrapMode.Loop : WrapMode.Once;
            }
        }

        int firstActionIndex = idleActionIndex >= 0 ? idleActionIndex : 0;

        if (firstActionIndex >= 0 && firstActionIndex < runtimeActions.Length)
        {
            currentLegacyActionIndex = firstActionIndex;
            legacyAnimation.Play(runtimeActions[firstActionIndex].LegacyClipName);
        }
    }

    private void UpdateWeights()
    {
        int targetIndex = GetTargetActionIndex();
        float step = blendSpeed * Time.deltaTime;
        currentActionId = targetIndex >= 0 && targetIndex < runtimeActions.Length ? runtimeActions[targetIndex].ActionId : "";

        if (logActionChanges && targetIndex != lastLoggedActionIndex)
        {
            lastLoggedActionIndex = targetIndex;
            Debug.Log(gameObject.name + ": animation action -> " + currentActionId + " speed=" + currentPlanarSpeed.ToString("0.00"));
        }

        if (playbackBackend == PlaybackBackend.LegacyAnimationComponent)
        {
            UpdateLegacyAnimation(targetIndex);
            return;
        }

        if (playbackBackend == PlaybackBackend.ProceduralBodyMotion)
        {
            UpdateProceduralBodyMotion(targetIndex);
            return;
        }

        for (int i = 0; i < runtimeActions.Length; i++)
        {
            RuntimeClipAction action = runtimeActions[i];
            float targetWeight = i == targetIndex ? 1f : 0f;
            action.Weight = Mathf.MoveTowards(action.Weight, targetWeight, step);
            runtimeActions[i] = action;
            mixer.SetInputWeight(i, action.Weight);
        }
    }

    private void UpdatePlayback()
    {
        if (playbackBackend != PlaybackBackend.PlayableGraph)
            return;

        for (int i = 0; i < runtimeActions.Length; i++)
        {
            RuntimeClipAction action = runtimeActions[i];

            if (!action.Playable.IsValid())
                continue;

            action.Playable.SetSpeed(action.PlaybackSpeed);

            if (action.Loop)
                LoopPlayable(action.Playable, action.Clip);
            else
                ClampPlayable(action.Playable, action.Clip);
        }
    }

    private void UpdateProceduralBodyMotion(int targetIndex)
    {
        bool isWalking = targetIndex == walkActionIndex && currentPlanarSpeed > walkThreshold;
        float targetBlend = isWalking ? 1f : 0f;
        locomotionBlend = Mathf.MoveTowards(locomotionBlend, targetBlend, blendSpeed * Time.deltaTime);

        float frequency = Mathf.Lerp(idleFrequency, walkFrequency, locomotionBlend);
        motionPhase += Time.deltaTime * frequency;

        float idleBob = Mathf.Sin(motionPhase) * idleBobAmplitude * (1f - locomotionBlend);
        float walkBob = Mathf.Abs(Mathf.Sin(motionPhase)) * walkBobAmplitude * locomotionBlend;
        float swing = Mathf.Sin(motionPhase) * walkSwingDegrees * locomotionBlend;

        for (int i = 0; i < bodyPartPoses.Length; i++)
        {
            BodyPartPose pose = bodyPartPoses[i];

            if (pose.Transform == null)
                continue;

            pose.Transform.localPosition = pose.BaseLocalPosition + Vector3.up * (idleBob + walkBob);

            float pitch = 0f;
            float roll = 0f;

            switch (pose.Role)
            {
                case BodyPartRole.LeftArm:
                    pitch = swing;
                    roll = 4f * locomotionBlend;
                    break;
                case BodyPartRole.RightArm:
                    pitch = -swing;
                    roll = -4f * locomotionBlend;
                    break;
                case BodyPartRole.LeftLeg:
                    pitch = -swing;
                    break;
                case BodyPartRole.RightLeg:
                    pitch = swing;
                    break;
                case BodyPartRole.Body:
                    roll = Mathf.Sin(motionPhase * 0.5f) * 2f * locomotionBlend;
                    break;
            }

            pose.Transform.localRotation = pose.BaseLocalRotation * Quaternion.Euler(pitch, 0f, roll);
        }
    }

    private void CacheBodyParts(Transform searchRoot)
    {
        bodyPartPoses = new BodyPartPose[0];
        AddBodyPart(searchRoot, "Arm_L", BodyPartRole.LeftArm);
        AddBodyPart(searchRoot, "Arm_R", BodyPartRole.RightArm);
        AddBodyPart(searchRoot, "Leg_L", BodyPartRole.LeftLeg);
        AddBodyPart(searchRoot, "Leg_R", BodyPartRole.RightLeg);
        AddBodyPart(searchRoot, "Head", BodyPartRole.Body);
        AddBodyPart(searchRoot, "Torso", BodyPartRole.Body);
        AddBodyPart(searchRoot, "Weist", BodyPartRole.Body);
    }

    private void AddBodyPart(Transform searchRoot, string name, BodyPartRole role)
    {
        Transform bodyPart = FindChildByName(searchRoot, name);

        if (bodyPart == null)
            return;

        int index = bodyPartPoses.Length;
        Array.Resize(ref bodyPartPoses, bodyPartPoses.Length + 1);
        bodyPartPoses[index] = new BodyPartPose
        {
            Transform = bodyPart,
            BaseLocalPosition = bodyPart.localPosition,
            BaseLocalRotation = bodyPart.localRotation,
            Role = role
        };
    }

    private static Transform FindChildByName(Transform root, string name)
    {
        if (root == null)
            return null;

        if (string.Equals(root.name, name, StringComparison.OrdinalIgnoreCase))
            return root;

        for (int i = 0; i < root.childCount; i++)
        {
            Transform found = FindChildByName(root.GetChild(i), name);

            if (found != null)
                return found;
        }

        return null;
    }

    private void UpdateLegacyAnimation(int targetIndex)
    {
        if (legacyAnimation == null || targetIndex < 0 || targetIndex >= runtimeActions.Length)
            return;

        RuntimeClipAction action = runtimeActions[targetIndex];
        AnimationState state = legacyAnimation[action.LegacyClipName];

        if (state != null)
            state.speed = action.PlaybackSpeed;

        if (targetIndex == currentLegacyActionIndex && legacyAnimation.IsPlaying(action.LegacyClipName))
            return;

        currentLegacyActionIndex = targetIndex;
        legacyAnimation.CrossFade(action.LegacyClipName, Mathf.Max(0.01f, 1f / Mathf.Max(0.01f, blendSpeed)));
    }

    private int GetTargetActionIndex()
    {
        if (!string.IsNullOrWhiteSpace(forceActionId))
        {
            int forcedInspectorIndex = FindActionIndex(forceActionId);

            if (forcedInspectorIndex >= 0)
                return forcedInspectorIndex;
        }

        if (forcedActionIndex >= 0 && forcedActionIndex < runtimeActions.Length)
            return forcedActionIndex;

        currentPlanarSpeed = GetPlanarSpeed();

        if (walkActionIndex >= 0 && currentPlanarSpeed > walkThreshold)
            return walkActionIndex;

        return idleActionIndex >= 0 ? idleActionIndex : 0;
    }

    private int FindActionIndex(string actionId)
    {
        if (string.IsNullOrWhiteSpace(actionId))
            return -1;

        for (int i = 0; i < runtimeActions.Length; i++)
        {
            if (string.Equals(runtimeActions[i].ActionId, actionId, StringComparison.OrdinalIgnoreCase))
                return i;
        }

        return -1;
    }

    private float GetPlanarSpeed()
    {
        float speed = 0f;

        if (parentRigidbody != null)
        {
#if UNITY_6000_0_OR_NEWER
            Vector3 velocity = parentRigidbody.linearVelocity;
#else
            Vector3 velocity = parentRigidbody.velocity;
#endif
            velocity.y = 0f;
            speed = velocity.magnitude;
        }

        if (useTransformDeltaSpeed && Time.deltaTime > 0f)
        {
            Vector3 delta = transform.position - previousPosition;
            previousPosition = transform.position;
            delta.y = 0f;
            speed = Mathf.Max(speed, delta.magnitude / Time.deltaTime);
        }

        return speed;
    }

    private static bool IsUsableClip(AnimationClip clip)
    {
        return clip != null && clip.length > 0.01f && !clip.name.StartsWith("__preview__", StringComparison.OrdinalIgnoreCase);
    }

    private static AnimationClip ChooseClip(AnimationClip[] clips, string exactClipName, string nameContains)
    {
        if (clips == null || clips.Length == 0)
            return null;

        if (!string.IsNullOrWhiteSpace(exactClipName))
        {
            for (int i = 0; i < clips.Length; i++)
            {
                if (string.Equals(clips[i].name, exactClipName, StringComparison.OrdinalIgnoreCase))
                    return clips[i];
            }
        }

        if (!string.IsNullOrWhiteSpace(nameContains))
        {
            for (int i = 0; i < clips.Length; i++)
            {
                if (clips[i].name.IndexOf(nameContains, StringComparison.OrdinalIgnoreCase) >= 0)
                    return clips[i];
            }
        }

        return null;
    }

    private static void LoopPlayable(AnimationClipPlayable playable, AnimationClip clip)
    {
        if (!playable.IsValid() || clip == null || clip.length <= 0.01f)
            return;

        double time = playable.GetTime();

        if (time >= clip.length)
            playable.SetTime(time % clip.length);
    }

    private static void ClampPlayable(AnimationClipPlayable playable, AnimationClip clip)
    {
        if (!playable.IsValid() || clip == null || clip.length <= 0.01f)
            return;

        if (playable.GetTime() >= clip.length)
        {
            playable.SetTime(clip.length);
            playable.SetSpeed(0f);
        }
    }

    private string BuildLoadedClipLog()
    {
        string result = gameObject.name + ": loaded embedded animation clips";

        for (int i = 0; i < clips.Length; i++)
            result += "\n- " + clips[i].name + " (" + clips[i].length.ToString("0.00") + "s)";

        result += "\nActions:";

        for (int i = 0; i < runtimeActions.Length; i++)
            result += "\n- " + runtimeActions[i].ActionId + " -> " + (runtimeActions[i].Clip == null ? "procedural" : runtimeActions[i].Clip.name);

        result += "\nBackend: " + playbackBackend;
        result += "\nBody Parts: " + bodyPartPoses.Length;
        result += "\nAnimator Avatar: " + (animator == null || animator.avatar == null ? "none" : animator.avatar.name + " (valid: " + animator.avatar.isValid + ")");
        result += "\nAnimator Root: " + (animator == null ? "none" : animator.gameObject.name);

        return result;
    }
}
