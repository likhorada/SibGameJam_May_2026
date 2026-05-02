using UnityEngine;

/// <summary>
/// Описание элемента: ID, имя, UI-иконка, цвет-заглушка и 3D-префаб для отображения в мире.
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

    [Header("UI Visual")]
    [SerializeField] private Sprite icon;
    [SerializeField] private Color fallbackColor = Color.gray;

    [Header("World Visual")]
    [SerializeField] private GameObject worldPrefab;
    [SerializeField] private Vector3 worldScale = Vector3.one;

    [Header("Rules")]
    [SerializeField] private bool discardOnTableClose;

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

    public GameObject WorldPrefab
    {
        get { return worldPrefab; }
    }

    public Vector3 WorldScale
    {
        get { return worldScale; }
    }

    /// <summary>
    /// Если true, элемент исчезает при закрытии крафтового стола и не возвращается в инвентарь.
    /// Используется для глины голема.
    /// </summary>
    public bool DiscardOnTableClose
    {
        get { return discardOnTableClose; }
    }

    private void OnValidate()
    {
        if (!string.IsNullOrEmpty(id))
            id = id.Trim().ToLowerInvariant().Replace(" ", "_");
    }
}