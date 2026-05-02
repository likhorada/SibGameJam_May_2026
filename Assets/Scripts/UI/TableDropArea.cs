using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Свободное поле стола. Принимает элементы из инвентаря и перемещает элементы по столу.
/// </summary>
public sealed class TableDropArea : MonoBehaviour, IDropHandler
{
    private CraftingPanelUI craftingPanel;
    private RectTransform rectTransform;

    public void Configure(CraftingPanelUI craftingPanel)
    {
        this.craftingPanel = craftingPanel;
        rectTransform = transform as RectTransform;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (!DragContext.HasActiveDrag)
            return;

        Vector2 localPoint = GetLocalPoint(eventData);

        if (DragContext.SourceType == DragSourceType.InventorySlot)
        {
            bool removedFromInventory = Inventory.Instance.TryClearSlot(DragContext.InventorySlotIndex);

            if (!removedFromInventory)
                return;

            craftingPanel.CreateTableItem(DragContext.ElementId, localPoint);

            DragContext.MarkHandled();
            return;
        }

        if (DragContext.SourceType == DragSourceType.TableItem)
        {
            DragContext.TableItem.AttachToTable(localPoint);
            DragContext.MarkHandled();
        }
    }

    private Vector2 GetLocalPoint(PointerEventData eventData)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint
        );

        return localPoint;
    }
}