using System.Collections.Generic;

/// <summary>
/// Запись справочника: вид элемента и отображаемое имя.
/// </summary>
public sealed class ElementCatalogEntry
{
    public ElementKind ElementKind { get; }
    public string Name { get; }

    public ElementCatalogEntry(ElementKind elementKind, string name)
    {
        ElementKind = elementKind;
        Name = name;
    }
}

/// <summary>
/// Единый справочник: тип элемента и отображаемое имя.
/// </summary>
public static class ElementCatalog
{
    public static readonly IReadOnlyDictionary<ElementKind, ElementCatalogEntry> EntriesByKind =
        new Dictionary<ElementKind, ElementCatalogEntry>
        {
            [ElementKind.Fire] = new ElementCatalogEntry(ElementKind.Fire, "Fire"),
            [ElementKind.Stone] = new ElementCatalogEntry(ElementKind.Stone, "Stone"),
            [ElementKind.KeyCore] = new ElementCatalogEntry(ElementKind.KeyCore, "Key Core"),
            [ElementKind.RoomTwoResult] = new ElementCatalogEntry(ElementKind.RoomTwoResult, "Room Two Result"),
            [ElementKind.Clay] = new ElementCatalogEntry(ElementKind.Clay, "Clay")
        };

    public static string GetDisplayName(ElementKind kind)
    {
        if (kind == ElementKind.None)
            return string.Empty;

        return EntriesByKind.TryGetValue(kind, out ElementCatalogEntry entry) ? entry.Name : kind.ToString();
    }
}
