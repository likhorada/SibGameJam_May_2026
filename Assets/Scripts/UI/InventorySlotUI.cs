using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// UI одного слота инвентаря. Один слот хранит один элемент.
/// </summary>
public sealed class InventorySlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    private static readonly Dictionary<ElementKind, Color> ElementColors = new Dictionary<ElementKind, Color>
    {
        [ElementKind.Fire] = new Color(0.9f, 0.22f, 0.08f, 1f),
        [ElementKind.Stone] = new Color(0.5f, 0.5f, 0.5f, 1f),
        [ElementKind.KeyCore] = new Color(0.95f, 0.78f, 0.18f, 1f),
        [ElementKind.RoomTwoResult] = new Color(0.35f, 0.75f, 1f, 1f),
        [ElementKind.Clay] = new Color(0.65f, 0.36f, 0.18f, 1f),
    };

    private static readonly Color DefaultElementColor = new Color(0.3f, 0.3f, 0.3f, 1f);

    private int slotIndex;
    private Canvas rootCanvas;
    private Text label;
    private Image background;

    private GameObject dragGhost;
    private RectTransform dragGhostRect;

    private ElementKind elementId;

    public void Configure(int slotIndex, Canvas rootCanvas, Text label, Image background)
    {
        this.slotIndex = slotIndex;
        this.rootCanvas = rootCanvas;
        this.label = label;
        this.background = background;

        SetEmpty();
    }

    public void SetData(InventorySlotData data)
    {
        if (data == null || data.IsEmpty)
        {
            SetEmpty();
            return;
        }

        elementId = data.ElementId;

        label.text = data.ElementName;
        background.color = GetColorForElement(elementId);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (elementId == ElementKind.None)
            return;

        DragContext.BeginFromInventory(slotIndex, elementId);
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

        if (elementId != ElementKind.None)
            return;

        bool placed = Inventory.Instance.TrySetSlot(
            slotIndex,
            DragContext.ElementId
        );

        if (!placed)
            return;

        DragContext.TableItem.DestroySelf();
        DragContext.MarkHandled();
    }

    private void SetEmpty()
    {
        elementId = ElementKind.None;

        if (label != null)
            label.text = "Empty";

        if (background != null)
            background.color = new Color(0.18f, 0.18f, 0.18f, 1f);
    }

    private void CreateDragGhost(PointerEventData eventData)
    {
        dragGhost = new GameObject("InventoryDragGhost");
        dragGhost.transform.SetParent(rootCanvas.transform, false);

        dragGhostRect = dragGhost.AddComponent<RectTransform>();
        dragGhostRect.sizeDelta = new Vector2(150f, 70f);

        Image image = dragGhost.AddComponent<Image>();
        image.color = GetColorForElement(elementId);
        image.raycastTarget = false;

        Text text = UIFactory.CreateText(
            parent: dragGhost.transform,
            name: "Text",
            value: ElementCatalog.GetDisplayName(elementId),
            fontSize: 15,
            alignment: TextAnchor.MiddleCenter,
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

    public static Color GetColorForElement(ElementKind kind)
    {
        if (kind != ElementKind.None && ElementColors.TryGetValue(kind, out Color color))
            return color;

        return DefaultElementColor;
    }
}