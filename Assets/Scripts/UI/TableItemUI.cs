using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Элемент, лежащий на столе.
/// Его можно двигать, соединять с другим элементом или вернуть в инвентарь.
/// </summary>
public sealed class TableItemUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    private Canvas rootCanvas;
    private CraftingPanelUI craftingPanel;
    private RectTransform tableArea;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    private Vector2 previousTablePosition;
    private Vector2 originalSizeDelta;
    private bool destroyedByDrop;

    public ElementDefinition Element { get; private set; }

    /// <summary>
    /// true — элемент вернётся в инвентарь при закрытии стола.
    /// false — элемент исчезнет при закрытии стола.
    /// </summary>
    public bool ShouldReturnToInventoryOnClose { get; private set; }

    public void Configure(
        Canvas rootCanvas,
        CraftingPanelUI craftingPanel,
        RectTransform tableArea,
        ElementDefinition element,
        bool shouldReturnToInventoryOnClose)
    {
        this.rootCanvas = rootCanvas;
        this.craftingPanel = craftingPanel;
        this.tableArea = tableArea;
        this.Element = element;
        this.ShouldReturnToInventoryOnClose = shouldReturnToInventoryOnClose;

        rectTransform = transform as RectTransform;
        originalSizeDelta = rectTransform.sizeDelta;

        canvasGroup = gameObject.AddComponent<CanvasGroup>();

        ResetRectTransform();

        Image image = GetComponent<Image>();
        image.color = element.FallbackColor;

        if (element.Icon != null)
            image.sprite = element.Icon;

        Text label = UIFactory.CreateText(
            parent: transform,
            name: "Label",
            value: element.DisplayName,
            fontSize: 13,
            alignment: TextAnchor.LowerCenter,
            anchorMin: Vector2.zero,
            anchorMax: Vector2.one,
            pivot: new Vector2(0.5f, 0.5f),
            anchoredPosition: Vector2.zero,
            sizeDelta: Vector2.zero
        );

        label.raycastTarget = false;
    }

    public Vector2 GetTablePosition()
    {
        return rectTransform.anchoredPosition;
    }

    public void AttachToTable(Vector2 tablePosition)
    {
        transform.SetParent(tableArea, false);

        ResetRectTransform();

        rectTransform.anchoredPosition = tablePosition;
        canvasGroup.blocksRaycasts = true;
    }

    public void DestroySelf()
    {
        destroyedByDrop = true;
        craftingPanel.UnregisterTableItem(this);
        Destroy(gameObject);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        previousTablePosition = rectTransform.anchoredPosition;

        DragContext.BeginFromTable(this, Element);

        canvasGroup.blocksRaycasts = false;

        // Важно: не сохраняем world scale при переносе в Canvas.
        // Иначе после возврата элемент может стать меньше/больше.
        transform.SetParent(rootCanvas.transform, false);

        ResetRectTransform();
        MoveToPointer(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        MoveToPointer(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (destroyedByDrop)
        {
            DragContext.Clear();
            return;
        }

        if (!DragContext.WasHandled)
            AttachToTable(previousTablePosition);

        canvasGroup.blocksRaycasts = true;
        DragContext.Clear();
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (!DragContext.HasActiveDrag)
            return;

        Vector2 resultPosition = GetTablePosition();

        bool crafted = false;

        if (DragContext.SourceType == DragSourceType.TableItem)
        {
            TableItemUI draggedItem = DragContext.TableItem;

            if (draggedItem == null || draggedItem == this)
                return;

            crafted = craftingPanel.TryCraftTableItems(
                draggedItem,
                this,
                resultPosition
            );
        }
        else if (DragContext.SourceType == DragSourceType.InventorySlot)
        {
            crafted = craftingPanel.TryCraftIncomingElement(
                DragContext.Element,
                this,
                resultPosition,
                consumeInventorySlot: true,
                inventorySlotIndex: DragContext.InventorySlotIndex
            );
        }
        else if (DragContext.SourceType == DragSourceType.PermanentElementSlot)
        {
            crafted = craftingPanel.TryCraftIncomingElement(
                DragContext.Element,
                this,
                resultPosition,
                consumeInventorySlot: false,
                inventorySlotIndex: -1
            );
        }

        if (crafted)
            DragContext.MarkHandled();
    }

    private void MoveToPointer(PointerEventData eventData)
    {
        RectTransform canvasRect = rootCanvas.transform as RectTransform;

        bool converted = RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            eventData.position,
            rootCanvas.worldCamera,
            out Vector2 localPoint
        );

        if (converted)
            rectTransform.anchoredPosition = localPoint;
    }

    private void ResetRectTransform()
    {
        rectTransform.localScale = Vector3.one;
        rectTransform.localRotation = Quaternion.identity;
        rectTransform.sizeDelta = originalSizeDelta;
    }
}
