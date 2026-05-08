using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

public sealed class PauseMenuController : MonoBehaviour
{
    public static bool IsPaused { get; private set; }

    [Header("Layout")]
    [SerializeField] private Vector2 panelSize = new Vector2(420f, 430f);
    [SerializeField] private Vector2 buttonSize = new Vector2(260f, 52f);
    [SerializeField] private Vector2 sliderSize = new Vector2(260f, 22f);

    [Header("Visual")]
    [SerializeField] private UIImageStyle backdropStyle =
        UIImageStyle.Create(new Color(0.02f, 0.018f, 0.015f, 0.78f), true);
    [SerializeField] private UIImageStyle panelStyle =
        UIImageStyle.Create(new Color(0.07f, 0.06f, 0.048f, 0.96f), true);
    [SerializeField] private UIImageStyle buttonStyle =
        UIImageStyle.Create(new Color(0.18f, 0.18f, 0.18f, 1f), true);
    [SerializeField] private UIImageStyle sliderBackgroundStyle =
        UIImageStyle.Create(new Color(0.15f, 0.14f, 0.12f, 1f), true);
    [SerializeField] private UIImageStyle sliderFillStyle =
        UIImageStyle.Create(new Color(0.62f, 0.49f, 0.25f, 1f), true);
    [SerializeField] private UIImageStyle sliderHandleStyle =
        UIImageStyle.Create(new Color(0.9f, 0.78f, 0.46f, 1f), true);

    [Header("Text")]
    [SerializeField] private string titleText = "Paused";
    [SerializeField] private int titleFontSize = 34;
    [SerializeField] private int labelFontSize = 18;
    [SerializeField] private int buttonFontSize = 18;
    [SerializeField] private Color textColor = Color.white;

    [Header("Audio")]
    [SerializeField, Range(0f, 1f)] private float musicVolume = 1f;
    [SerializeField, Range(0f, 1f)] private float sfxVolume = 1f;

    private Canvas rootCanvas;
    private GameObject overlay;
    private bool isBuilt;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        IsPaused = false;
    }

    public void Configure(Canvas canvas)
    {
        rootCanvas = canvas;
        ForceRootRect();

        if (!isBuilt)
            BuildUI();

        SetMenuVisible(false);
        ApplyAudioVolumes();
    }

    private void ForceRootRect()
    {
        if (rootCanvas != null && !transform.IsChildOf(rootCanvas.transform))
            transform.SetParent(rootCanvas.transform, false);

        RectTransform rectTransform = transform as RectTransform;

        if (rectTransform == null)
            rectTransform = gameObject.AddComponent<RectTransform>();

        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = Vector2.zero;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        transform.SetAsLastSibling();
    }

    private void Awake()
    {
        ApplyAudioVolumes();
    }

    private void Update()
    {
        if (GameOverController.IsGameOver)
            return;

        if (!Input.GetKeyDown(KeyCode.Escape))
            return;

        if (CraftingPanelUI.LastEscapeCloseFrame == Time.frameCount)
            return;

        if (CraftingPanelUI.HasOpenPanel)
            return;

        if (IsPaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void PauseGame()
    {
        if (IsPaused)
            return;

        IsPaused = true;
        Time.timeScale = 0f;
        SetMenuVisible(true);
    }

    public void ResumeGame()
    {
        if (!IsPaused)
            return;

        IsPaused = false;
        Time.timeScale = 1f;
        SetMenuVisible(false);
    }

    public void RestartGame()
    {
        IsPaused = false;
        Time.timeScale = 1f;

        Scene activeScene = SceneManager.GetActiveScene();

        if (activeScene.buildIndex >= 0)
        {
            SceneManager.LoadScene(activeScene.buildIndex);
            return;
        }

        SceneManager.LoadScene(activeScene.name);
    }

    public void QuitGame()
    {
        IsPaused = false;
        Time.timeScale = 1f;

#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        AmbientMusicSwitcher.SetMusicVolume(musicVolume);
    }

    public void SetSfxVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        GameAudio.SetMasterVolume(sfxVolume);
    }

    private void ApplyAudioVolumes()
    {
        AmbientMusicSwitcher.SetMusicVolume(musicVolume);
        GameAudio.SetMasterVolume(sfxVolume);
    }

    private void SetMenuVisible(bool visible)
    {
        if (overlay != null)
            overlay.SetActive(visible);
    }

    private void BuildUI()
    {
        isBuilt = true;

        if (rootCanvas == null)
            rootCanvas = FindAnyObjectByType<Canvas>();

        if (rootCanvas == null)
        {
            Debug.LogError("PauseMenuController: root canvas not found");
            return;
        }

        overlay = new GameObject("PauseMenuOverlay");
        overlay.transform.SetParent(transform, false);
        overlay.transform.SetAsLastSibling();

        RectTransform overlayRect = overlay.AddComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.pivot = new Vector2(0.5f, 0.5f);
        overlayRect.anchoredPosition = Vector2.zero;
        overlayRect.sizeDelta = Vector2.zero;

        Image backdrop = overlay.AddComponent<Image>();
        EnsureStyle(ref backdropStyle, new Color(0.02f, 0.018f, 0.015f, 0.78f), true);
        backdropStyle.ApplyTo(backdrop);

        GameObject panel = new GameObject("Panel");
        panel.transform.SetParent(overlay.transform, false);

        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = panelSize;

        Image panelImage = panel.AddComponent<Image>();
        EnsureStyle(ref panelStyle, new Color(0.07f, 0.06f, 0.048f, 0.96f), true);
        panelStyle.ApplyTo(panelImage);

        Text title = UIFactory.CreateText(
            parent: panel.transform,
            name: "Title",
            value: titleText,
            fontSize: titleFontSize,
            alignment: TextAnchor.MiddleCenter,
            anchorMin: new Vector2(0.5f, 1f),
            anchorMax: new Vector2(0.5f, 1f),
            pivot: new Vector2(0.5f, 1f),
            anchoredPosition: new Vector2(0f, -28f),
            sizeDelta: new Vector2(340f, 54f)
        );
        title.color = textColor;

        CreateSliderRow(panel.transform, "Music", new Vector2(0f, 100f), musicVolume, SetMusicVolume);
        CreateSliderRow(panel.transform, "SFX", new Vector2(0f, 36f), sfxVolume, SetSfxVolume);
        CreateButton(panel.transform, "ResumeButton", "Resume", new Vector2(0f, -58f), ResumeGame);
        CreateButton(panel.transform, "RestartButton", "Restart", new Vector2(0f, -122f), RestartGame);
        CreateButton(panel.transform, "QuitButton", "Quit", new Vector2(0f, -186f), QuitGame);
    }

    private void CreateButton(
        Transform parent,
        string name,
        string label,
        Vector2 position,
        UnityEngine.Events.UnityAction action)
    {
        Button button = UIFactory.CreateButton(
            parent: parent,
            name: name,
            label: label,
            anchorMin: new Vector2(0.5f, 0.5f),
            anchorMax: new Vector2(0.5f, 0.5f),
            pivot: new Vector2(0.5f, 0.5f),
            anchoredPosition: position,
            sizeDelta: buttonSize
        );

        button.onClick.AddListener(action);

        Image image = button.GetComponent<Image>();
        EnsureStyle(ref buttonStyle, new Color(0.18f, 0.18f, 0.18f, 1f), true);
        buttonStyle.ApplyTo(image);

        Text text = button.GetComponentInChildren<Text>();
        if (text != null)
        {
            text.fontSize = buttonFontSize;
            text.color = textColor;
        }
    }

    private void CreateSliderRow(
        Transform parent,
        string label,
        Vector2 position,
        float value,
        UnityEngine.Events.UnityAction<float> onChanged)
    {
        GameObject row = new GameObject(label + "VolumeRow");
        row.transform.SetParent(parent, false);

        RectTransform rowRect = row.AddComponent<RectTransform>();
        rowRect.anchorMin = new Vector2(0.5f, 0.5f);
        rowRect.anchorMax = new Vector2(0.5f, 0.5f);
        rowRect.pivot = new Vector2(0.5f, 0.5f);
        rowRect.anchoredPosition = position;
        rowRect.sizeDelta = new Vector2(340f, 52f);

        Text labelText = UIFactory.CreateText(
            parent: row.transform,
            name: "Label",
            value: label,
            fontSize: labelFontSize,
            alignment: TextAnchor.MiddleLeft,
            anchorMin: new Vector2(0f, 0.5f),
            anchorMax: new Vector2(0f, 0.5f),
            pivot: new Vector2(0f, 0.5f),
            anchoredPosition: Vector2.zero,
            sizeDelta: new Vector2(74f, 36f)
        );
        labelText.color = textColor;

        Slider slider = CreateSlider(row.transform, label + "Slider", value);

        RectTransform sliderRect = slider.transform as RectTransform;
        sliderRect.anchorMin = new Vector2(1f, 0.5f);
        sliderRect.anchorMax = new Vector2(1f, 0.5f);
        sliderRect.pivot = new Vector2(1f, 0.5f);
        sliderRect.anchoredPosition = Vector2.zero;
        sliderRect.sizeDelta = sliderSize;

        slider.onValueChanged.AddListener(onChanged);
    }

    private Slider CreateSlider(Transform parent, string name, float value)
    {
        GameObject sliderObject = new GameObject(name);
        sliderObject.transform.SetParent(parent, false);

        RectTransform sliderRect = sliderObject.AddComponent<RectTransform>();
        sliderRect.sizeDelta = sliderSize;

        Slider slider = sliderObject.AddComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = Mathf.Clamp01(value);

        EnsureStyle(ref sliderBackgroundStyle, new Color(0.15f, 0.14f, 0.12f, 1f), true);
        GameObject backgroundObject = CreateSliderImage(sliderObject.transform, "Background", sliderBackgroundStyle);
        RectTransform backgroundRect = backgroundObject.transform as RectTransform;
        backgroundRect.anchorMin = new Vector2(0f, 0.5f);
        backgroundRect.anchorMax = new Vector2(1f, 0.5f);
        backgroundRect.pivot = new Vector2(0.5f, 0.5f);
        backgroundRect.anchoredPosition = Vector2.zero;
        backgroundRect.sizeDelta = new Vector2(0f, 8f);

        GameObject fillAreaObject = new GameObject("Fill Area");
        fillAreaObject.transform.SetParent(sliderObject.transform, false);

        RectTransform fillAreaRect = fillAreaObject.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = new Vector2(0f, 0.5f);
        fillAreaRect.anchorMax = new Vector2(1f, 0.5f);
        fillAreaRect.pivot = new Vector2(0.5f, 0.5f);
        fillAreaRect.anchoredPosition = Vector2.zero;
        fillAreaRect.sizeDelta = new Vector2(-18f, 8f);

        EnsureStyle(ref sliderFillStyle, new Color(0.62f, 0.49f, 0.25f, 1f), true);
        GameObject fillObject = CreateSliderImage(fillAreaObject.transform, "Fill", sliderFillStyle);
        RectTransform fillRect = fillObject.transform as RectTransform;
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.pivot = new Vector2(0f, 0.5f);
        fillRect.anchoredPosition = Vector2.zero;
        fillRect.sizeDelta = Vector2.zero;

        GameObject handleAreaObject = new GameObject("Handle Slide Area");
        handleAreaObject.transform.SetParent(sliderObject.transform, false);

        RectTransform handleAreaRect = handleAreaObject.AddComponent<RectTransform>();
        handleAreaRect.anchorMin = Vector2.zero;
        handleAreaRect.anchorMax = Vector2.one;
        handleAreaRect.pivot = new Vector2(0.5f, 0.5f);
        handleAreaRect.anchoredPosition = Vector2.zero;
        handleAreaRect.sizeDelta = new Vector2(-18f, 0f);

        EnsureStyle(ref sliderHandleStyle, new Color(0.9f, 0.78f, 0.46f, 1f), true);
        GameObject handleObject = CreateSliderImage(handleAreaObject.transform, "Handle", sliderHandleStyle);
        RectTransform handleRect = handleObject.transform as RectTransform;
        handleRect.sizeDelta = new Vector2(18f, 18f);

        slider.fillRect = fillRect;
        slider.handleRect = handleRect;
        slider.targetGraphic = handleObject.GetComponent<Image>();
        slider.direction = Slider.Direction.LeftToRight;

        return slider;
    }

    private static GameObject CreateSliderImage(Transform parent, string name, UIImageStyle style)
    {
        GameObject imageObject = new GameObject(name);
        imageObject.transform.SetParent(parent, false);

        imageObject.AddComponent<RectTransform>();

        Image image = imageObject.AddComponent<Image>();
        style.ApplyTo(image);

        return imageObject;
    }

    private static void EnsureStyle(ref UIImageStyle style, Color fallbackColor, bool raycastTarget)
    {
        if (style == null)
            style = UIImageStyle.Create(fallbackColor, raycastTarget);
    }
}
