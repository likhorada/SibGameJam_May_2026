using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// UI одного слота инвентаря.
/// Может быть обычным слотом или вечным слотом элемента, например глины.
/// </summary>
public sealed class InventorySlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    private int slotIndex;
    private bool isPermanentElementSlot;

    private Canvas rootCanvas;
    private Image background;
    private Image icon;
    private Text label;

    private GameObject dragGhost;
    private RectTransform dragGhostRect;

    private ElementDefinition element;

    public void ConfigureNormalSlot(
        int slotIndex,
        Canvas rootCanvas,
        Image background,
        Image icon,
        Text label)
    {
        this.slotIndex = slotIndex;
        this.rootCanvas = rootCanvas;
        this.background = background;
        this.icon = icon;
        this.label = label;
        this.isPermanentElementSlot = false;

        SetEmpty();
    }

    public void ConfigurePermanentElementSlot(
        Canvas rootCanvas,
        Image background,
        Image icon,
        Text label,
        ElementDefinition permanentElement)
    {
        this.slotIndex = -1;
        this.rootCanvas = rootCanvas;
        this.background = background;
        this.icon = icon;
        this.label = label;
        this.isPermanentElementSlot = true;

        SetElement(permanentElement);
    }

    public void SetData(InventorySlotData data)
    {
        if (isPermanentElementSlot)
            return;

        if (data == null || data.IsEmpty)
        {
            SetEmpty();
            return;
        }

        SetElement(data.Element);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (element == null)
            return;

        if (isPermanentElementSlot)
            DragContext.BeginFromPermanentElement(element);
        else
            DragContext.BeginFromInventory(slotIndex, element);

        CreateDragGhost(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        MoveDragGhost(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        DestroyDragGhost();
        DragContext.Clear();
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (!DragContext.HasActiveDrag)
            return;

        if (DragContext.SourceType != DragSourceType.TableItem)
            return;

        if (isPermanentElementSlot)
            return;

        if (element != null)
            return;

        TableItemUI tableItem = DragContext.TableItem;

        if (tableItem == null)
            return;

        if (!tableItem.ShouldReturnToInventoryOnClose)
            return;

        if (DragContext.Element == null || DragContext.Element.DiscardOnTableClose)
            return;

        bool placed = Inventory.Instance.TrySetSlot(slotIndex, DragContext.Element);

        if (!placed)
            return;

        tableItem.DestroySelf();
        DragContext.MarkHandled();
    }

    private void SetElement(ElementDefinition newElement)
    {
        element = newElement;

        if (element == null)
        {
            SetEmpty();
            return;
        }

        label.text = element.DisplayName;
        background.color = element.FallbackColor;

        if (icon == null)
            return;

        if (element.Icon != null)
        {
            icon.sprite = element.Icon;
            icon.color = Color.white;
            icon.enabled = true;
        }
        else
        {
            icon.sprite = null;
            icon.enabled = false;
        }
    }

    private void SetEmpty()
    {
        element = null;

        if (label != null)
            label.text = "Empty";

        if (background != null)
            background.color = new Color(0.18f, 0.18f, 0.18f, 1f);

        if (icon != null)
        {
            icon.sprite = null;
            icon.enabled = false;
        }
    }

    private void CreateDragGhost(PointerEventData eventData)
    {
        dragGhost = new GameObject("InventoryDragGhost");
        dragGhost.transform.SetParent(rootCanvas.transform, false);

        dragGhostRect = dragGhost.AddComponent<RectTransform>();
        dragGhostRect.sizeDelta = new Vector2(120f, 80f);

        Image image = dragGhost.AddComponent<Image>();
        image.color = element.FallbackColor;
        image.raycastTarget = false;

        if (element.Icon != null)
            image.sprite = element.Icon;

        Text text = UIFactory.CreateText(
            parent: dragGhost.transform,
            name: "Text",
            value: element.DisplayName,
            fontSize: 14,
            alignment: TextAnchor.LowerCenter,
            anchorMin: Vector2.zero,
            anchorMax: Vector2.one,
            pivot: new Vector2(0.5f, 0.5f),
            anchoredPosition: Vector2.zero,
            sizeDelta: Vector2.zero
        );

        text.raycastTarget = false;

        MoveDragGhost(eventData);
    }

    private void MoveDragGhost(PointerEventData eventData)
    {
        if (dragGhostRect == null)
            return;

        RectTransform canvasRect = rootCanvas.transform as RectTransform;

        bool converted = RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            eventData.position,
            rootCanvas.worldCamera,
            out Vector2 localPoint
        );

        if (converted)
            dragGhostRect.anchoredPosition = localPoint;
    }

    private void DestroyDragGhost()
    {
        if (dragGhost != null)
            Destroy(dragGhost);

        dragGhost = null;
        dragGhostRect = null;
    }
}
