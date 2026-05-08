using UnityEngine;
using UnityEngine.UI;

public enum InteractionHintPlacement
{
    ScreenCenter,
    InteractableWorldPosition,
    CustomWorldPosition,
    CustomScreenPosition
}

public struct InteractionHintRequest
{
    public string Message;
    public InteractionHintPlacement Placement;
    public Transform Target;
    public Vector3 WorldPosition;
    public Vector3 WorldOffset;
    public Vector2 ScreenOffset;
    public float Duration;
    public InteractionHintVisualOverrides VisualOverrides;
}

[System.Serializable]
public struct InteractionHintVisualOverrides
{
    public bool OverrideWindowSize;
    public Vector2 WindowSize;
    public bool OverrideWindowStyle;
    public UIImageStyle WindowStyle;
    public bool OverrideTextColor;
    public Color TextColor;
    public bool OverrideFontSize;
    public int FontSize;
    public bool OverrideTextPadding;
    public Vector2 TextPadding;

    public bool HasAnyOverride
    {
        get
        {
            return OverrideWindowSize
                || OverrideWindowStyle
                || OverrideTextColor
                || OverrideFontSize
                || OverrideTextPadding;
        }
    }
}

public sealed class InteractionHintWindow : MonoBehaviour
{
    private static InteractionHintWindow instance;

    [Header("Canvas")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private Camera worldCamera;
    [SerializeField] private int sortingOrder = 250;

    [Header("Window Visual")]
    [SerializeField] private Vector2 windowSize = new Vector2(420f, 118f);
    [SerializeField] private UIImageStyle windowStyle =
        UIImageStyle.Create(new Color(0f, 0f, 0f, 0f), false);
    [SerializeField] private Color textColor = Color.white;
    [SerializeField] private int fontSize = 20;
    [SerializeField] private Vector2 textPadding = new Vector2(24f, 16f);

    private RectTransform canvasRect;
    private RectTransform windowRect;
    private Image backgroundImage;
    private Text bodyText;

    private InteractionHintRequest activeRequest;
    private float hideAtTime;
    private bool showing;

    public static void Show(InteractionHintRequest request)
    {
        InteractionHintWindow window = GetOrCreate();
        window.ShowInternal(request);
    }

    private static InteractionHintWindow GetOrCreate()
    {
        if (instance != null)
            return instance;

        instance = FindAnyObjectByType<InteractionHintWindow>();

        if (instance != null)
            return instance;

        GameObject windowObject = new GameObject("InteractionHintWindow");
        instance = windowObject.AddComponent<InteractionHintWindow>();
        return instance;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        BuildIfNeeded();
        Hide();
    }

    private void Update()
    {
        if (!showing)
            return;

        if (Time.unscaledTime >= hideAtTime)
        {
            Hide();
            return;
        }

        RefreshPosition();
    }

    private void ShowInternal(InteractionHintRequest request)
    {
        BuildIfNeeded();

        activeRequest = request;
        hideAtTime = Time.unscaledTime + Mathf.Max(0.1f, request.Duration);
        showing = true;

        ApplyVisualOverrides(request.VisualOverrides);
        bodyText.text = request.Message;
        windowRect.gameObject.SetActive(true);
        RefreshPosition();
    }

    private void Hide()
    {
        showing = false;

        if (windowRect != null)
            windowRect.gameObject.SetActive(false);
    }

    private void BuildIfNeeded()
    {
        if (canvas == null)
            CreateCanvas();

        canvasRect = canvas.transform as RectTransform;

        if (windowRect != null)
            return;

        GameObject panelObject = new GameObject("HintPanel");
        panelObject.transform.SetParent(canvas.transform, false);

        windowRect = panelObject.AddComponent<RectTransform>();
        windowRect.anchorMin = new Vector2(0.5f, 0.5f);
        windowRect.anchorMax = new Vector2(0.5f, 0.5f);
        windowRect.pivot = new Vector2(0.5f, 0.5f);
        windowRect.sizeDelta = windowSize;

        backgroundImage = panelObject.AddComponent<Image>();

        if (windowStyle == null)
            windowStyle = UIImageStyle.Create(new Color(0f, 0f, 0f, 0f), false);

        windowStyle.ApplyTo(backgroundImage);
        backgroundImage.raycastTarget = false;

        GameObject textObject = new GameObject("Text");
        textObject.transform.SetParent(panelObject.transform, false);

        RectTransform textRect = textObject.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = textPadding;
        textRect.offsetMax = -textPadding;

        bodyText = textObject.AddComponent<Text>();
        bodyText.font = UIFactory.DefaultFont;
        bodyText.color = textColor;
        bodyText.fontSize = fontSize;
        bodyText.alignment = TextAnchor.MiddleCenter;
        bodyText.horizontalOverflow = HorizontalWrapMode.Wrap;
        bodyText.verticalOverflow = VerticalWrapMode.Truncate;
        bodyText.raycastTarget = false;
    }

    private void ApplyVisualOverrides(InteractionHintVisualOverrides visualOverrides)
    {
        Vector2 resolvedWindowSize = visualOverrides.OverrideWindowSize
            ? visualOverrides.WindowSize
            : windowSize;

        windowRect.sizeDelta = resolvedWindowSize;

        UIImageStyle resolvedWindowStyle = visualOverrides.OverrideWindowStyle
            ? visualOverrides.WindowStyle
            : windowStyle;

        if (resolvedWindowStyle == null)
            resolvedWindowStyle = UIImageStyle.Create(new Color(0f, 0f, 0f, 0f), false);

        resolvedWindowStyle.ApplyTo(backgroundImage);
        backgroundImage.raycastTarget = false;

        bodyText.color = visualOverrides.OverrideTextColor
            ? visualOverrides.TextColor
            : textColor;

        bodyText.fontSize = visualOverrides.OverrideFontSize
            ? Mathf.Max(1, visualOverrides.FontSize)
            : fontSize;

        Vector2 resolvedPadding = visualOverrides.OverrideTextPadding
            ? visualOverrides.TextPadding
            : textPadding;

        RectTransform textRect = bodyText.transform as RectTransform;

        if (textRect != null)
        {
            textRect.offsetMin = resolvedPadding;
            textRect.offsetMax = -resolvedPadding;
        }
    }

    private void CreateCanvas()
    {
        GameObject canvasObject = new GameObject("InteractionHintCanvas");
        canvasObject.transform.SetParent(transform, false);

        canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>().enabled = false;
    }

    private void RefreshPosition()
    {
        if (windowRect == null || canvasRect == null)
            return;

        switch (activeRequest.Placement)
        {
            case InteractionHintPlacement.InteractableWorldPosition:
                SetWorldPosition(GetTargetWorldPosition());
                break;
            case InteractionHintPlacement.CustomWorldPosition:
                SetWorldPosition(GetTargetWorldPosition());
                break;
            case InteractionHintPlacement.CustomScreenPosition:
                windowRect.anchoredPosition = activeRequest.ScreenOffset;
                break;
            default:
                windowRect.anchoredPosition = activeRequest.ScreenOffset;
                break;
        }
    }

    private Vector3 GetTargetWorldPosition()
    {
        if (activeRequest.Target == null)
            return activeRequest.WorldPosition + activeRequest.WorldOffset;

        return activeRequest.Target.position + activeRequest.WorldOffset;
    }

    private void SetWorldPosition(Vector3 worldPosition)
    {
        Camera cameraToUse = worldCamera != null ? worldCamera : Camera.main;

        if (cameraToUse == null)
        {
            windowRect.anchoredPosition = activeRequest.ScreenOffset;
            return;
        }

        Vector3 screenPoint = cameraToUse.WorldToScreenPoint(worldPosition);

        if (screenPoint.z < 0f)
        {
            windowRect.gameObject.SetActive(false);
            return;
        }

        windowRect.gameObject.SetActive(true);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPoint,
            canvas.worldCamera,
            out Vector2 localPoint
        );

        windowRect.anchoredPosition = localPoint + activeRequest.ScreenOffset;
    }
}
