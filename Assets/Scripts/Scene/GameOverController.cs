using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class GameOverController : MonoBehaviour
{
    public static GameOverController Instance { get; private set; }

    public static bool IsGameOver { get; private set; }

    [SerializeField] private Canvas rootCanvas;
    [SerializeField] private string titleText = "GAME OVER";
    [SerializeField] private string messageText = "The golem was destroyed by the explosion.";
    [SerializeField] private string restartButtonText = "Restart";
    [SerializeField] private Color backdropColor = new Color(0.04f, 0.025f, 0.02f, 0.92f);
    [SerializeField] private bool pauseGameOnGameOver = true;

    private GameObject overlay;
    private bool isGameOver;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("GameOverController: duplicate instance found, disabling duplicate");
            enabled = false;
            return;
        }

        Instance = this;
        IsGameOver = false;
    }

    public void TriggerGameOver()
    {
        if (isGameOver)
            return;

        isGameOver = true;
        IsGameOver = true;
        BuildOverlay();

        if (pauseGameOnGameOver)
            Time.timeScale = 0f;
    }

    private void BuildOverlay()
    {
        if (overlay != null)
            return;

        if (rootCanvas == null)
            rootCanvas = FindAnyObjectByType<Canvas>();

        if (rootCanvas == null)
        {
            Debug.LogError("GameOverController: root canvas not found");
            return;
        }

        overlay = new GameObject("GameOverOverlay");
        overlay.transform.SetParent(rootCanvas.transform, false);
        overlay.transform.SetAsLastSibling();

        RectTransform overlayRect = overlay.AddComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.pivot = new Vector2(0.5f, 0.5f);
        overlayRect.anchoredPosition = Vector2.zero;
        overlayRect.sizeDelta = Vector2.zero;

        Image backdrop = overlay.AddComponent<Image>();
        backdrop.color = backdropColor;
        backdrop.raycastTarget = true;

        UIFactory.CreateText(
            parent: overlay.transform,
            name: "Title",
            value: titleText,
            fontSize: 54,
            alignment: TextAnchor.MiddleCenter,
            anchorMin: new Vector2(0.5f, 0.5f),
            anchorMax: new Vector2(0.5f, 0.5f),
            pivot: new Vector2(0.5f, 0.5f),
            anchoredPosition: new Vector2(0f, 90f),
            sizeDelta: new Vector2(760f, 80f)
        );

        UIFactory.CreateText(
            parent: overlay.transform,
            name: "Message",
            value: messageText,
            fontSize: 22,
            alignment: TextAnchor.MiddleCenter,
            anchorMin: new Vector2(0.5f, 0.5f),
            anchorMax: new Vector2(0.5f, 0.5f),
            pivot: new Vector2(0.5f, 0.5f),
            anchoredPosition: new Vector2(0f, 20f),
            sizeDelta: new Vector2(760f, 70f)
        );

        Button restartButton = UIFactory.CreateButton(
            parent: overlay.transform,
            name: "RestartButton",
            label: restartButtonText,
            anchorMin: new Vector2(0.5f, 0.5f),
            anchorMax: new Vector2(0.5f, 0.5f),
            pivot: new Vector2(0.5f, 0.5f),
            anchoredPosition: new Vector2(0f, -80f),
            sizeDelta: new Vector2(220f, 56f)
        );

        restartButton.onClick.AddListener(RestartGame);
    }

    private void RestartGame()
    {
        Time.timeScale = 1f;
        IsGameOver = false;

        Scene activeScene = SceneManager.GetActiveScene();

        if (activeScene.buildIndex >= 0)
        {
            SceneManager.LoadScene(activeScene.buildIndex);
            return;
        }

        SceneManager.LoadScene(activeScene.name);
    }
}
