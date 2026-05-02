/// <summary>
/// Тип источника текущего drag and drop.
/// </summary>
public enum DragSourceType
{
    None,
    InventorySlot,
    TableItem
}

/// <summary>
/// Общий контекст текущего перетаскивания.
/// UI-слоты и стол читают отсюда, что именно сейчас тащат.
/// </summary>
public static class DragContext
{
    public static bool HasActiveDrag { get; private set; }
    public static bool WasHandled { get; private set; }

    public static DragSourceType SourceType { get; private set; }

    public static ElementKind ElementId { get; private set; }

    public static int InventorySlotIndex { get; private set; }
    public static TableItemUI TableItem { get; private set; }

    public static void BeginFromInventory(int slotIndex, ElementKind elementId)
    {
        HasActiveDrag = true;
        WasHandled = false;
        SourceType = DragSourceType.InventorySlot;

        InventorySlotIndex = slotIndex;
        TableItem = null;

        ElementId = elementId;
    }

    public static void BeginFromTable(TableItemUI tableItem, ElementKind elementId)
    {
        HasActiveDrag = true;
        WasHandled = false;
        SourceType = DragSourceType.TableItem;

        InventorySlotIndex = -1;
        TableItem = tableItem;

        ElementId = elementId;
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

        ElementId = ElementKind.None;

        InventorySlotIndex = -1;
        TableItem = null;
    }
}