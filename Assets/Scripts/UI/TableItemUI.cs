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
    private bool destroyedByDrop;

    public ElementDefinition Element { get; private set; }

    /// <summary>
    /// true — элемент вернётся в инвентарь при закрытии стола.
    /// false — элемент исчезнет при закрытии стола.
    /// Для вечной глины должно быть false.
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
        canvasGroup = gameObject.AddComponent<CanvasGroup>();

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

        if (draggedItem == null || draggedItem == this)
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