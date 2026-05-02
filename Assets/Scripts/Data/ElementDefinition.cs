using UnityEngine;

/// <summary>
/// Описание элемента: ID, имя, иконка и цвет-заглушка.
/// Это данные, которые удобно настраивать через Inspector.
/// </summary>
[CreateAssetMenu(
    fileName = "ElementDefinition",
    menuName = "Golem Craft/Element Definition"
)]
public sealed class ElementDefinition : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private string id;
    [SerializeField] private string displayName;

    [Header("Visual")]
    [SerializeField] private Sprite icon;
    [SerializeField] private Color fallbackColor = Color.gray;

    public string Id
    {
        get { return id; }
    }

    public string DisplayName
    {
        get { return displayName; }
    }

    public Sprite Icon
    {
        get { return icon; }
    }

    public Color FallbackColor
    {
        get { return fallbackColor; }
    }

    private void OnValidate()
    {
        if (!string.IsNullOrEmpty(id))
            id = id.Trim().ToLowerInvariant().Replace(" ", "_");
    }
}