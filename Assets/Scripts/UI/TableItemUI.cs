using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Элемент, лежащий на столе. Его можно двигать, крафтить с другим элементом и возвращать в инвентарь.
/// </summary>
public sealed class TableItemUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    private Canvas rootCanvas;
    private CraftingPanelUI craftingPanel;
    private RectTransform tableArea;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    private Vector2 previousTablePosition;
    private bool destroyedByDrop;

    public InventoryItemType ElementId { get; private set; }

    public void Configure(
        Canvas rootCanvas,
        CraftingPanelUI craftingPanel,
        RectTransform tableArea,
        InventoryItemType elementId)
    {
        this.rootCanvas = rootCanvas;
        this.craftingPanel = craftingPanel;
        this.tableArea = tableArea;

        ElementId = elementId;

        rectTransform = transform as RectTransform;
        canvasGroup = gameObject.AddComponent<CanvasGroup>();

        Image image = GetComponent<Image>();
        if (image != null)
            image.color = InventorySlotUI.GetColorForElement(elementId);

        Text label = UIFactory.CreateText(
            parent: transform,
            name: "Label",
            value: Items.GetDisplayName(elementId),
            fontSize: 15,
            alignment: TextAnchor.MiddleCenter,
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
        rectTransform.anchoredPosition = tablePosition;
        canvasGroup.blocksRaycasts = true;
    }

    public void DestroySelf()
    {
        destroyedByDrop = true;
        Destroy(gameObject);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        previousTablePosition = rectTransform.anchoredPosition;

        DragContext.BeginFromTable(this, ElementId);

        canvasGroup.blocksRaycasts = false;
        transform.SetParent(rootCanvas.transform, true);

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

        if (DragContext.SourceType != DragSourceType.TableItem)
            return;

        TableItemUI draggedItem = DragContext.TableItem;

        if (draggedItem == null)
            return;

        if (draggedItem == this)
            return;

        Vector2 resultPosition = GetTablePosition();

        bool crafted = craftingPanel.TryCraftTableItems(
            draggedItem,
            this,
            resultPosition
        );

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
}