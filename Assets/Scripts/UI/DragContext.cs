/// <summary>
/// Тип источника текущего drag and drop.
/// </summary>
public enum DragSourceType
{
    None,
    InventorySlot,
    PermanentElementSlot,
    TableItem
}

/// <summary>
/// Общий контекст текущего перетаскивания.
/// </summary>
public static class DragContext
{
    public static bool HasActiveDrag { get; private set; }
    public static bool WasHandled { get; private set; }

    public static DragSourceType SourceType { get; private set; }
    public static ElementDefinition Element { get; private set; }

    public static int InventorySlotIndex { get; private set; }
    public static TableItemUI TableItem { get; private set; }

    public static void BeginFromInventory(int slotIndex, ElementDefinition element)
    {
        HasActiveDrag = true;
        WasHandled = false;
        SourceType = DragSourceType.InventorySlot;

        InventorySlotIndex = slotIndex;
        TableItem = null;
        Element = element;
    }

    public static void BeginFromPermanentElement(ElementDefinition element)
    {
        HasActiveDrag = true;
        WasHandled = false;
        SourceType = DragSourceType.PermanentElementSlot;

        InventorySlotIndex = -1;
        TableItem = null;
        Element = element;
    }

    public static void BeginFromTable(TableItemUI tableItem, ElementDefinition element)
    {
        HasActiveDrag = true;
        WasHandled = false;
        SourceType = DragSourceType.TableItem;

        InventorySlotIndex = -1;
        TableItem = tableItem;
        Element = element;
    }

    public static void MarkHandled()
    {
        WasHandled = true;
    }

    public static void Clear()
    {
        HasActiveDrag = false;
        WasHandled = false;
        SourceType = DragSourceType.None;

        Element = null;
        InventorySlotIndex = -1;
        TableItem = null;
    }
}