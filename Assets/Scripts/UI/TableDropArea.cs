using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Свободное поле стола.
/// Принимает элементы из обычного инвентаря, вечного слота глины и перемещает элементы стола.
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

            // Обычный элемент должен вернуться в инвентарь при закрытии стола.
            craftingPanel.CreateTableItem(
                DragContext.Element,
                localPoint,
                shouldReturnToInventoryOnClose: true
            );

            DragContext.MarkHandled();
            return;
        }

        if (DragContext.SourceType == DragSourceType.PermanentElementSlot)
        {
            // Вечная глина не возвращается в инвентарь.
            // Она просто исчезает при закрытии стола.
            craftingPanel.CreateTableItem(
                DragContext.Element,
                localPoint,
                shouldReturnToInventoryOnClose: false
            );

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