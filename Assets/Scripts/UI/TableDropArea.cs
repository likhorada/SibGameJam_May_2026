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

        if (craftingPanel == null)
        {
            Debug.LogError("TableDropArea: CraftingPanelUI is not configured");
            return;
        }

        if (DragContext.Element == null)
            return;

        if (!TryGetLocalPoint(eventData, out Vector2 localPoint))
            return;

        if (DragContext.SourceType == DragSourceType.InventorySlot)
        {
            if (Inventory.Instance == null)
            {
                Debug.LogError("TableDropArea: Inventory instance not found");
                return;
            }

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
            TableItemUI tableItem = DragContext.TableItem;

            if (tableItem == null)
                return;

            tableItem.AttachToTable(localPoint);
            DragContext.MarkHandled();
        }
    }

    private bool TryGetLocalPoint(PointerEventData eventData, out Vector2 localPoint)
    {
        localPoint = Vector2.zero;

        if (rectTransform == null)
        {
            Debug.LogError("TableDropArea: RectTransform is missing");
            return false;
        }

        return RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out localPoint
        );
    }
}
