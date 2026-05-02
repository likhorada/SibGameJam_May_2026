/// <summary>
/// Элемент инвентаря.
/// </summary>
public sealed class InventoryItem
{
    public InventoryItemType Type { get; }

    public string Name { get; }

    public InventoryItem(InventoryItemType type, string name)
    {
        Type = type;
        Name = name;
    }
}