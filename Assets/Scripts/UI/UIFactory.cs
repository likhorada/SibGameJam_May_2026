using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Маленькая фабрика UI, чтобы не собирать Canvas руками как первобытный шаман.
/// </summary>
public static class UIFactory
{
    private static Font cachedFont;

    public static Font DefaultFont
    {
        get
        {
            if (cachedFont != null)
                return cachedFont;

            cachedFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            if (cachedFont == null)
                cachedFont = Resources.GetBuiltinResource<Font>("Arial.ttf");

            return cachedFont;
        }
    }

    public static GameObject CreatePanel(
        Transform parent,
        string name,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 pivot,
        Vector2 anchoredPosition,
        Vector2 sizeDelta,
        Color color)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(parent, false);

        RectTransform rectTransform = panel.AddComponent<RectTransform>();
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.pivot = pivot;
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = sizeDelta;

        Image image = panel.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = true;

        return panel;
    }

    public static Text CreateText(
        Transform parent,
        string name,
        string value,
        int fontSize,
        TextAnchor alignment,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 pivot,
        Vector2 anchoredPosition,
        Vector2 sizeDelta)
    {
        GameObject textObject = new GameObject(name);
        textObject.transform.SetParent(parent, false);

        RectTransform rectTransform = textObject.AddComponent<RectTransform>();
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.pivot = pivot;
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = sizeDelta;

        Text text = textObject.AddComponent<Text>();
        text.font = DefaultFont;
        text.text = value;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = Color.white;
        text.raycastTarget = true;

        return text;
    }

    public static Button CreateButton(
        Transform parent,
        string name,
        string label,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 pivot,
        Vector2 anchoredPosition,
        Vector2 sizeDelta)
    {
        GameObject buttonObject = new GameObject(name);
        buttonObject.transform.SetParent(parent, false);

        RectTransform rectTransform = buttonObject.AddComponent<RectTransform>();
        rectTransform.anchorMin = anchorMin;
        rectTransform.anchorMax = anchorMax;
        rectTransform.pivot = pivot;
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.sizeDelta = sizeDelta;

        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(0.18f, 0.18f, 0.18f, 1f);
        image.raycastTarget = true;

        Button button = buttonObject.AddComponent<Button>();

        Text text = CreateText(
            parent: buttonObject.transform,
            name: "Text",
            value: label,
            fontSize: 18,
            alignment: TextAnchor.MiddleCenter,
            anchorMin: Vector2.zero,
            anchorMax: Vector2.one,
            pivot: new Vector2(0.5f, 0.5f),
            anchoredPosition: Vector2.zero,
            sizeDelta: Vector2.zero
        );

        text.raycastTarget = false;

        return button;
    }
}