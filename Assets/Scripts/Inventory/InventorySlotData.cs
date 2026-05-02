/// <summary>
/// Данные одного слота инвентаря. Один слот хранит только один элемент.
/// </summary>
public sealed class InventorySlotData
{
    public InventoryItemType ElementId { get; private set; }

    public string ElementName
    {
        get { return Items.GetDisplayName(ElementId); }
    }

    public bool IsEmpty
    {
        get { return ElementId == InventoryItemType.None; }
    }

    public void Set(InventoryItemType elementId)
    {
        ElementId = elementId;
    }

    public void Clear()
    {
        ElementId = InventoryItemType.None;
    }
}