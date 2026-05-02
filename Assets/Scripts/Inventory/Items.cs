using System.Collections.Generic;

/// <summary>
/// Единый справочник: тип элемента и отображаемое имя.
/// </summary>
public static class Items
{
    public static readonly IReadOnlyDictionary<InventoryItemType, InventoryItem> EntriesByKind =
        new Dictionary<InventoryItemType, InventoryItem>
        {
            [InventoryItemType.Fire] = new InventoryItem(InventoryItemType.Fire, "Fire"),
            [InventoryItemType.Stone] = new InventoryItem(InventoryItemType.Stone, "Stone"),
            [InventoryItemType.KeyCore] = new InventoryItem(InventoryItemType.KeyCore, "Key Core"),
            [InventoryItemType.RoomTwoResult] = new InventoryItem(InventoryItemType.RoomTwoResult, "Room Two Result"),
            [InventoryItemType.Clay] = new InventoryItem(InventoryItemType.Clay, "Clay")
        };

    public static string GetDisplayName(InventoryItemType kind)
    {
        if (kind == InventoryItemType.None)
            return string.Empty;

        return EntriesByKind.TryGetValue(kind, out InventoryItem entry) ? entry.Name : kind.ToString();
    }
}
