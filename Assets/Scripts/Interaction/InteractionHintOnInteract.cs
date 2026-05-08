using System;
using UnityEngine;

public enum InteractionHintCondition
{
    Always,
    Inactive,
    InactiveMissingRequiredItem,
    InactiveHasRequiredItem,
    ActivatedByThisInteraction,
    AlreadyActive
}

public interface IInteractionHintStateProvider
{
    bool IsHintStateActive { get; }
    ElementDefinition RequiredHintElement { get; }
}

public sealed class InteractionHintContext
{
    private readonly IInteractable interactable;

    public IInteractionHintStateProvider StateProvider { get; private set; }
    public bool WasActiveBefore { get; private set; }
    public bool IsActiveAfter { get; private set; }
    public bool HadRequiredItemBefore { get; private set; }

    public bool ActivatedByThisInteraction
    {
        get
        {
            return StateProvider != null
                && !WasActiveBefore
                && IsActiveAfter;
        }
    }

    private InteractionHintContext(IInteractable interactable)
    {
        this.interactable = interactable;
    }

    public static InteractionHintContext CaptureBefore(IInteractable interactable)
    {
        InteractionHintContext context = new InteractionHintContext(interactable);
        context.StateProvider = ResolveStateProvider(interactable);

        if (context.StateProvider == null)
            return context;

        context.WasActiveBefore = context.StateProvider.IsHintStateActive;
        context.IsActiveAfter = context.StateProvider.IsHintStateActive;
        context.HadRequiredItemBefore = HasRequiredItem(context.StateProvider);

        return context;
    }

    public void CaptureAfter()
    {
        IInteractionHintStateProvider currentProvider =
            StateProvider ?? ResolveStateProvider(interactable);

        if (currentProvider == null)
            return;

        StateProvider = currentProvider;
        IsActiveAfter = currentProvider.IsHintStateActive;
    }

    private static IInteractionHintStateProvider ResolveStateProvider(IInteractable interactable)
    {
        IInteractionHintStateProvider directProvider =
            interactable as IInteractionHintStateProvider;

        if (directProvider != null)
            return directProvider;

        Component component = interactable as Component;

        if (component == null)
            return null;

        IInteractionHintStateProvider provider =
            component.GetComponent<IInteractionHintStateProvider>();

        if (provider != null)
            return provider;

        return component.GetComponentInParent<IInteractionHintStateProvider>();
    }

    private static bool HasRequiredItem(IInteractionHintStateProvider provider)
    {
        if (provider == null || provider.RequiredHintElement == null)
            return false;

        if (Inventory.Instance == null)
            return false;

        return Inventory.Instance.HasElement(provider.RequiredHintElement);
    }
}

[Serializable]
public sealed class InteractionHintRule
{
    [SerializeField] private InteractionHintCondition condition = InteractionHintCondition.Always;
    [SerializeField, TextArea(2, 5)] private string message = "Hint text";
    [SerializeField] private int maxShowCount = 1;
    [SerializeField] private float duration = 2.5f;
    [SerializeField] private InteractionHintVisualSettings visualOverride = new InteractionHintVisualSettings();

    private int showCount;

    public string Message
    {
        get { return message; }
    }

    public float Duration
    {
        get { return duration; }
    }

    public InteractionHintVisualSettings VisualOverride
    {
        get { return visualOverride; }
    }

    public bool CanShow(InteractionHintContext context)
    {
        if (string.IsNullOrWhiteSpace(message))
            return false;

        if (maxShowCount > 0 && showCount >= maxShowCount)
            return false;

        return Matches(context);
    }

    public void MarkShown()
    {
        showCount++;
    }

    public void ResetShowCount()
    {
        showCount = 0;
    }

    private bool Matches(InteractionHintContext context)
    {
        switch (condition)
        {
            case InteractionHintCondition.Inactive:
                return context != null
                    && context.StateProvider != null
                    && !context.WasActiveBefore;
            case InteractionHintCondition.InactiveMissingRequiredItem:
                return context != null
                    && context.StateProvider != null
                    && !context.WasActiveBefore
                    && context.StateProvider.RequiredHintElement != null
                    && !context.HadRequiredItemBefore;
            case InteractionHintCondition.InactiveHasRequiredItem:
                return context != null
                    && context.StateProvider != null
                    && !context.WasActiveBefore
                    && context.StateProvider.RequiredHintElement != null
                    && context.HadRequiredItemBefore;
            case InteractionHintCondition.ActivatedByThisInteraction:
                return context != null
                    && context.ActivatedByThisInteraction;
            case InteractionHintCondition.AlreadyActive:
                return context != null
                    && context.StateProvider != null
                    && context.WasActiveBefore;
            default:
                return true;
        }
    }
}

[Serializable]
public sealed class InteractionHintVisualSettings
{
    [SerializeField] private bool overrideWindowSize;
    [SerializeField] private Vector2 windowSize = new Vector2(420f, 118f);
    [SerializeField] private bool overrideWindowStyle;
    [SerializeField] private UIImageStyle windowStyle =
        UIImageStyle.Create(new Color(0f, 0f, 0f, 0.72f), false);
    [SerializeField] private bool overrideTextColor;
    [SerializeField] private Color textColor = Color.white;
    [SerializeField] private bool overrideFontSize;
    [SerializeField] private int fontSize = 20;
    [SerializeField] private bool overrideTextPadding;
    [SerializeField] private Vector2 textPadding = new Vector2(24f, 16f);

    public bool HasAnyOverride
    {
        get
        {
            return overrideWindowSize
                || overrideWindowStyle
                || overrideTextColor
                || overrideFontSize
                || overrideTextPadding;
        }
    }

    public InteractionHintVisualOverrides ToOverrides()
    {
        return new InteractionHintVisualOverrides
        {
            OverrideWindowSize = overrideWindowSize,
            WindowSize = windowSize,
            OverrideWindowStyle = overrideWindowStyle,
            WindowStyle = windowStyle,
            OverrideTextColor = overrideTextColor,
            TextColor = textColor,
            OverrideFontSize = overrideFontSize,
            FontSize = fontSize,
            OverrideTextPadding = overrideTextPadding,
            TextPadding = textPadding
        };
    }
}

[DisallowMultipleComponent]
public sealed class InteractionHintOnInteract : MonoBehaviour
{
    [Header("Default Hint")]
    [SerializeField, TextArea(2, 5)] private string message = "Hint text";

    [Header("Conditional Rules")]
    [SerializeField] private InteractionHintRule[] rules = new InteractionHintRule[0];

    [Header("When To Show")]
    [SerializeField] private int maxShowCount = 1;
    [SerializeField] private float duration = 2.5f;

    [Header("Placement")]
    [SerializeField] private InteractionHintPlacement placement =
        InteractionHintPlacement.InteractableWorldPosition;
    [SerializeField] private Transform customWorldAnchor;
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 1.25f, 0f);
    [SerializeField] private Vector2 screenOffset = new Vector2(0f, 110f);

    [Header("Visual Override")]
    [SerializeField] private InteractionHintVisualSettings visualOverride = new InteractionHintVisualSettings();

    private int showCount;

    public static void TryShowFor(
        IInteractable interactable,
        InteractionHintContext context)
    {
        Component component = interactable as Component;

        if (component == null)
            return;

        InteractionHintOnInteract hint = component.GetComponent<InteractionHintOnInteract>();

        if (hint == null)
            hint = component.GetComponentInParent<InteractionHintOnInteract>();

        if (hint != null)
            hint.Show(component.transform, context);
    }

    public void Show(Transform interactionTarget, InteractionHintContext context)
    {
        bool hasRules = rules != null && rules.Length > 0;
        InteractionHintRule rule = FindMatchingRule(context);

        if (rule != null)
        {
            rule.MarkShown();
            ShowMessage(interactionTarget, rule.Message, rule.Duration, rule.VisualOverride);
            return;
        }

        if (hasRules)
            return;

        ShowDefaultMessage(interactionTarget);
    }

    public void ResetShowCount()
    {
        showCount = 0;

        if (rules == null)
            return;

        for (int i = 0; i < rules.Length; i++)
        {
            if (rules[i] != null)
                rules[i].ResetShowCount();
        }
    }

    private InteractionHintRule FindMatchingRule(InteractionHintContext context)
    {
        if (rules == null || rules.Length == 0)
            return null;

        for (int i = 0; i < rules.Length; i++)
        {
            InteractionHintRule rule = rules[i];

            if (rule != null && rule.CanShow(context))
                return rule;
        }

        return null;
    }

    private void ShowDefaultMessage(Transform interactionTarget)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        if (maxShowCount > 0 && showCount >= maxShowCount)
            return;

        showCount++;
        ShowMessage(interactionTarget, message, duration, visualOverride);
    }

    private void ShowMessage(
        Transform interactionTarget,
        string text,
        float showDuration,
        InteractionHintVisualSettings ruleVisualOverride)
    {
        Transform target = ResolveTarget(interactionTarget);
        InteractionHintVisualSettings resolvedVisualOverride =
            ruleVisualOverride != null && ruleVisualOverride.HasAnyOverride
                ? ruleVisualOverride
                : visualOverride;

        InteractionHintRequest request = new InteractionHintRequest
        {
            Message = text,
            Placement = placement,
            Target = target,
            WorldPosition = target != null ? target.position : transform.position,
            WorldOffset = worldOffset,
            ScreenOffset = screenOffset,
            Duration = showDuration,
            VisualOverrides = resolvedVisualOverride != null
                ? resolvedVisualOverride.ToOverrides()
                : default
        };

        InteractionHintWindow.Show(request);
    }

    private Transform ResolveTarget(Transform interactionTarget)
    {
        if (placement == InteractionHintPlacement.CustomWorldPosition && customWorldAnchor != null)
            return customWorldAnchor;

        if (interactionTarget != null)
            return interactionTarget;

        return transform;
    }
}
