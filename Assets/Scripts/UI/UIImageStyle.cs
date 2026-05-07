using System;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public sealed class UIImageStyle
{
    [SerializeField] private Sprite sprite;
    [SerializeField] private Image.Type imageType = Image.Type.Sliced;
    [SerializeField] private Color color = Color.white;
    [SerializeField] private bool preserveAspect;
    [SerializeField] private bool raycastTarget = true;

    public static UIImageStyle Create(Color color, bool raycastTarget)
    {
        return new UIImageStyle
        {
            color = color,
            raycastTarget = raycastTarget
        };
    }

    public void ApplyTo(Image image)
    {
        if (image == null)
            return;

        image.sprite = sprite;
        image.type = sprite == null ? Image.Type.Simple : imageType;
        image.color = color;
        image.preserveAspect = preserveAspect;
        image.raycastTarget = raycastTarget;
    }
}

public enum ElementUIBackgroundMode
{
    None,
    FallbackColor,
    CustomStyle
}

public static class ElementUIBackgroundUtility
{
    private static readonly Color TransparentColor = new Color(0f, 0f, 0f, 0f);

    public static bool TryApplyPersonalBackground(
        ElementDefinition element,
        Image image,
        bool raycastTarget)
    {
        if (element == null || element.UiBackgroundMode == ElementUIBackgroundMode.None)
            return false;

        if (element.UiBackgroundMode == ElementUIBackgroundMode.CustomStyle)
        {
            ApplyStyle(element.UiBackgroundStyle, image, raycastTarget);
            return true;
        }

        ApplyFallbackColor(element, image, raycastTarget);
        return true;
    }

    public static void ApplyStyle(UIImageStyle style, Image image, bool raycastTarget)
    {
        if (image == null)
            return;

        if (style == null)
        {
            ApplyTransparent(image, raycastTarget);
            return;
        }

        image.enabled = true;
        style.ApplyTo(image);
        image.raycastTarget = raycastTarget;
    }

    public static void ApplyFallbackColor(
        ElementDefinition element,
        Image image,
        bool raycastTarget)
    {
        if (image == null)
            return;

        image.enabled = true;
        image.sprite = null;
        image.type = Image.Type.Simple;
        image.color = element != null ? element.FallbackColor : Color.gray;
        image.preserveAspect = false;
        image.raycastTarget = raycastTarget;
    }

    public static void ApplyTransparent(Image image, bool raycastTarget)
    {
        if (image == null)
            return;

        image.enabled = raycastTarget;
        image.sprite = null;
        image.type = Image.Type.Simple;
        image.color = TransparentColor;
        image.preserveAspect = false;
        image.raycastTarget = raycastTarget;
    }
}
