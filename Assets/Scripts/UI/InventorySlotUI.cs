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
    private Image elementBackground;
    private Image icon;
    private Text label;
    private Vector2 slotSize = new Vector2(124f, 104f);
    private Vector2 iconSize = new Vector2(72f, 72f);
    private Vector2 iconPosition = new Vector2(0f, 22f);
    private int dragLabelFontSize = 15;
    private float dragLabelHeight = 30f;

    private GameObject dragGhost;
    private RectTransform dragGhostRect;

    private ElementDefinition element;

    public void ConfigureNormalSlot(
        int slotIndex,
        Canvas rootCanvas,
        Image background,
        Image elementBackground,
        Image icon,
        Text label,
        Vector2 slotSize,
        Vector2 iconSize,
        Vector2 iconPosition,
        int dragLabelFontSize,
        float dragLabelHeight)
    {
        this.slotIndex = slotIndex;
        this.rootCanvas = rootCanvas;
        this.background = background;
        this.elementBackground = elementBackground;
        this.icon = icon;
        this.label = label;
        this.slotSize = slotSize;
        this.iconSize = iconSize;
        this.iconPosition = iconPosition;
        this.dragLabelFontSize = dragLabelFontSize;
        this.dragLabelHeight = dragLabelHeight;
        this.isPermanentElementSlot = false;

        SetEmpty();
    }

    public void ConfigurePermanentElementSlot(
        Canvas rootCanvas,
        Image background,
        Image elementBackground,
        Image icon,
        Text label,
        ElementDefinition permanentElement,
        Vector2 slotSize,
        Vector2 iconSize,
        Vector2 iconPosition,
        int dragLabelFontSize,
        float dragLabelHeight)
    {
        this.slotIndex = -1;
        this.rootCanvas = rootCanvas;
        this.background = background;
        this.elementBackground = elementBackground;
        this.icon = icon;
        this.label = label;
        this.slotSize = slotSize;
        this.iconSize = iconSize;
        this.iconPosition = iconPosition;
        this.dragLabelFontSize = dragLabelFontSize;
        this.dragLabelHeight = dragLabelHeight;
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

        GameAudio.Play(GameSoundId.UiDrag);
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

        if (isPermanentElementSlot)
            return;

        if (DragContext.SourceType == DragSourceType.InventorySlot)
        {
            MoveOrSwapInventorySlot();
            return;
        }

        if (DragContext.SourceType != DragSourceType.TableItem)
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
        GameAudio.Play(GameSoundId.UiDrop);
    }

    private void MoveOrSwapInventorySlot()
    {
        if (Inventory.Instance == null)
        {
            Debug.LogError("InventorySlotUI: Inventory instance not found");
            return;
        }

        bool moved = Inventory.Instance.TryMoveOrSwapSlots(
            DragContext.InventorySlotIndex,
            slotIndex
        );

        if (!moved)
            return;

        DragContext.MarkHandled();
        GameAudio.Play(GameSoundId.UiDrop);
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
        ApplyElementBackground();

        if (icon == null)
            return;

        if (element.Icon != null)
        {
            icon.sprite = element.Icon;
            icon.preserveAspect = true;
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

        if (icon != null)
        {
            icon.sprite = null;
            icon.enabled = false;
        }

        ElementUIBackgroundUtility.ApplyTransparent(elementBackground, false);
    }

    private void CreateDragGhost(PointerEventData eventData)
    {
        dragGhost = new GameObject("InventoryDragGhost");
        dragGhost.transform.SetParent(rootCanvas.transform, false);

        dragGhostRect = dragGhost.AddComponent<RectTransform>();
        dragGhostRect.sizeDelta = slotSize;

        Image backgroundImage = dragGhost.AddComponent<Image>();
        if (!ElementUIBackgroundUtility.TryApplyPersonalBackground(element, backgroundImage, false))
            ElementUIBackgroundUtility.ApplyTransparent(backgroundImage, false);

        if (element.Icon != null)
        {
            GameObject iconObject = new GameObject("Icon");
            iconObject.transform.SetParent(dragGhost.transform, false);

            RectTransform iconRect = iconObject.AddComponent<RectTransform>();
            iconRect.anchorMin = new Vector2(0.5f, 0.5f);
            iconRect.anchorMax = new Vector2(0.5f, 0.5f);
            iconRect.pivot = new Vector2(0.5f, 0.5f);
            iconRect.anchoredPosition = iconPosition;
            iconRect.sizeDelta = iconSize;

            Image iconImage = iconObject.AddComponent<Image>();
            iconImage.sprite = element.Icon;
            iconImage.preserveAspect = true;
            iconImage.color = Color.white;
            iconImage.raycastTarget = false;
        }

        Text text = UIFactory.CreateText(
            parent: dragGhost.transform,
            name: "Text",
            value: element.DisplayName,
            fontSize: dragLabelFontSize,
            alignment: TextAnchor.LowerCenter,
            anchorMin: new Vector2(0f, 0f),
            anchorMax: new Vector2(1f, 0f),
            pivot: new Vector2(0.5f, 0f),
            anchoredPosition: new Vector2(0f, 6f),
            sizeDelta: new Vector2(-10f, dragLabelHeight)
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

    private void ApplyElementBackground()
    {
        if (!ElementUIBackgroundUtility.TryApplyPersonalBackground(element, elementBackground, false))
            ElementUIBackgroundUtility.ApplyTransparent(elementBackground, false);
    }
}
