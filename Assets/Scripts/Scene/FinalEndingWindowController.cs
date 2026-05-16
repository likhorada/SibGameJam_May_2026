using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class FinalEndingWindowController : MonoBehaviour
{
    [Header("Canvas")]
    [SerializeField] private Canvas rootCanvas;

    [Header("Text")]
    [SerializeField] private string titleText = "THE GOLEM AWAKENS";
    [SerializeField] private string messageText = "Your work is complete.";
    [SerializeField] private string restartButtonText = "Restart";
    [SerializeField] private string quitButtonText = "Quit";
    [SerializeField] private int titleFontSize = 48;
    [SerializeField] private int messageFontSize = 22;
    [SerializeField] private int buttonFontSize = 18;
    [SerializeField] private Color textColor = Color.white;

    [Header("Layout")]
    [SerializeField] private Vector2 panelSize = new Vector2(720f, 360f);
    [SerializeField] private Vector2 buttonSize = new Vector2(180f, 54f);
    [SerializeField] private float buttonSpacing = 24f;

    [Header("Style")]
    [SerializeField] private UIImageStyle backdropStyle =
        UIImageStyle.Create(new Color(0.02f, 0.018f, 0.015f, 0.92f), true);
    [SerializeField] private UIImageStyle panelStyle =
        UIImageStyle.Create(new Color(0.12f, 0.105f, 0.085f, 0.96f), true);
    [SerializeField] private UIImageStyle buttonStyle =
        UIImageStyle.Create(new Color(0.26f, 0.22f, 0.16f, 1f), true);

    [Header("Behavior")]
    [SerializeField] private bool pauseGameOnShow = true;
    [SerializeField] private bool hideCursorLock = true;

    private GameObject overlay;
    private bool shown;

    public bool IsShown
    {
        get { return shown; }
    }

    public void ShowEnding()
    {
        if (shown)
            return;

        shown = true;
        BuildOverlay();

        if (overlay != null)
            overlay.SetActive(true);

        if (hideCursorLock)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (pauseGameOnShow)
            Time.timeScale = 0f;
    }

    public void HideEnding()
    {
        shown = false;

        if (overlay != null)
            overlay.SetActive(false);

        if (pauseGameOnShow)
            Time.timeScale = 1f;
    }

    public void RestartGame()
    {
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
        Time.timeScale = 1f;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void BuildOverlay()
    {
        if (overlay != null)
            return;

        if (rootCanvas == null)
            rootCanvas = FindAnyObjectByType<Canvas>();

        if (rootCanvas == null)
        {
            Debug.LogError("FinalEndingWindowController: root canvas not found");
            return;
        }

        overlay = new GameObject("FinalEndingOverlay");
        overlay.transform.SetParent(rootCanvas.transform, false);
        overlay.transform.SetAsLastSibling();

        RectTransform overlayRect = overlay.AddComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.pivot = new Vector2(0.5f, 0.5f);
        overlayRect.anchoredPosition = Vector2.zero;
        overlayRect.sizeDelta = Vector2.zero;

        Image backdrop = overlay.AddComponent<Image>();
        ApplyStyle(backdropStyle, backdrop);

        GameObject panel = new GameObject("Panel");
        panel.transform.SetParent(overlay.transform, false);

        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = Vector2.zero;
        panelRect.sizeDelta = panelSize;

        Image panelImage = panel.AddComponent<Image>();
        ApplyStyle(panelStyle, panelImage);

        Text title = UIFactory.CreateText(
            parent: panel.transform,
            name: "Title",
            value: titleText,
            fontSize: titleFontSize,
            alignment: TextAnchor.MiddleCenter,
            anchorMin: new Vector2(0.5f, 1f),
            anchorMax: new Vector2(0.5f, 1f),
            pivot: new Vector2(0.5f, 1f),
            anchoredPosition: new Vector2(0f, -46f),
            sizeDelta: new Vector2(panelSize.x - 80f, 72f)
        );
        title.color = textColor;

        Text message = UIFactory.CreateText(
            parent: panel.transform,
            name: "Message",
            value: messageText,
            fontSize: messageFontSize,
            alignment: TextAnchor.MiddleCenter,
            anchorMin: new Vector2(0.5f, 0.5f),
            anchorMax: new Vector2(0.5f, 0.5f),
            pivot: new Vector2(0.5f, 0.5f),
            anchoredPosition: new Vector2(0f, 20f),
            sizeDelta: new Vector2(panelSize.x - 90f, 120f)
        );
        message.color = textColor;

        float totalButtonWidth = buttonSize.x * 2f + buttonSpacing;
        float leftButtonX = -totalButtonWidth * 0.5f + buttonSize.x * 0.5f;
        float rightButtonX = totalButtonWidth * 0.5f - buttonSize.x * 0.5f;

        Button restartButton = CreateStyledButton(panel.transform, "RestartButton", restartButtonText, leftButtonX);
        restartButton.onClick.AddListener(RestartGame);

        Button quitButton = CreateStyledButton(panel.transform, "QuitButton", quitButtonText, rightButtonX);
        quitButton.onClick.AddListener(QuitGame);
    }

    private Button CreateStyledButton(Transform parent, string name, string label, float x)
    {
        Button button = UIFactory.CreateButton(
            parent: parent,
            name: name,
            label: label,
            anchorMin: new Vector2(0.5f, 0f),
            anchorMax: new Vector2(0.5f, 0f),
            pivot: new Vector2(0.5f, 0f),
            anchoredPosition: new Vector2(x, 44f),
            sizeDelta: buttonSize
        );

        Image image = button.GetComponent<Image>();
        ApplyStyle(buttonStyle, image);

        Text labelText = button.GetComponentInChildren<Text>();

        if (labelText != null)
        {
            labelText.fontSize = buttonFontSize;
            labelText.color = textColor;
        }

        return button;
    }

    private static void ApplyStyle(UIImageStyle style, Image image)
    {
        if (style != null)
            style.ApplyTo(image);
    }
}
