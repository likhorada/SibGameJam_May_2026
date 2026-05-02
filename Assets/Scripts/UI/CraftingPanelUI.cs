using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI конкретного стола крафта.
/// Сохраняет элементы на себе после закрытия.
/// </summary>
public sealed class CraftingPanelUI : MonoBehaviour
{
    private Canvas rootCanvas;
    private RectTransform tableArea;
    private Text titleText;
    private Text resultText;

    private string currentTableId;
    private string currentRoomId;
    private DoorInteractable linkedDoor;

    private bool isBuilt;

    private readonly List<TableItemUI> tableItems = new List<TableItemUI>();

    public void Configure(Canvas canvas)
    {
        rootCanvas = canvas;

        if (!isBuilt)
            BuildUI();
    }

    private void Update()
    {
        if (gameObject.activeSelf && Input.GetKeyDown(KeyCode.Escape))
            Close();
    }

    public void Open(string tableId, string roomId, DoorInteractable door)
    {
        currentTableId = tableId;
        currentRoomId = roomId;
        linkedDoor = door;

        gameObject.SetActive(true);

        titleText.text = "Craft Table / " + currentTableId + " / " + currentRoomId;
        resultText.text = "Drag inventory elements to the table";
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    public void CreateTableItem(ElementKind elementId, Vector2 tablePosition)
    {
        GameObject itemObject = new GameObject("TableItem_" + elementId);
        itemObject.transform.SetParent(tableArea, false);

        RectTransform rectTransform = itemObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(150f, 46f);
        rectTransform.anchoredPosition = tablePosition;

        Image image = itemObject.AddComponent<Image>();
        image.color = InventorySlotUI.GetColorForElement(elementId);
        image.raycastTarget = true;

        TableItemUI tableItem = itemObject.AddComponent<TableItemUI>();
        tableItem.Configure(
            rootCanvas,
            this,
            tableArea,
            elementId
        );

        tableItems.Add(tableItem);
    }

    public bool TryCraftTableItems(TableItemUI first, TableItemUI second, Vector2 resultPosition)
    {
        bool success = CraftingSystem.Instance.TryCraft(
            currentRoomId,
            first.ElementId,
            second.ElementId,
            out CraftResult result
        );

        if (!success)
        {
            resultText.text = "No recipe";
            return false;
        }

        tableItems.Remove(first);
        tableItems.Remove(second);

        first.DestroySelf();
        second.DestroySelf();

        CreateTableItem(result.ElementId, resultPosition);

        resultText.text = "Created: " + ElementCatalog.GetDisplayName(result.ElementId);

        if (linkedDoor != null)
            linkedDoor.NotifyCraftedElement(result.ElementId);

        return true;
    }

    private void BuildUI()
    {
        isBuilt = true;

        ClearChildren();

        titleText = UIFactory.CreateText(
            parent: transform,
            name: "Title",
            value: "Craft Table",
            fontSize: 24,
            alignment: TextAnchor.MiddleCenter,
            anchorMin: new Vector2(0.5f, 1f),
            anchorMax: new Vector2(0.5f, 1f),
            pivot: new Vector2(0.5f, 1f),
            anchoredPosition: new Vector2(0f, -14f),
            sizeDelta: new Vector2(650f, 40f)
        );

        GameObject tableObject = UIFactory.CreatePanel(
            parent: transform,
            name: "FreeTableArea",
            anchorMin: new Vector2(0.5f, 0.5f),
            anchorMax: new Vector2(0.5f, 0.5f),
            pivot: new Vector2(0.5f, 0.5f),
            anchoredPosition: new Vector2(0f, 0f),
            sizeDelta: new Vector2(620f, 320f),
            color: new Color(0.08f, 0.08f, 0.08f, 0.95f)
        );

        tableArea = tableObject.transform as RectTransform;

        TableDropArea dropArea = tableObject.AddComponent<TableDropArea>();
        dropArea.Configure(this);

        resultText = UIFactory.CreateText(
            parent: transform,
            name: "ResultText",
            value: "Drag inventory elements to the table",
            fontSize: 17,
            alignment: TextAnchor.MiddleCenter,
            anchorMin: new Vector2(0.5f, 0f),
            anchorMax: new Vector2(0.5f, 0f),
            pivot: new Vector2(0.5f, 0f),
            anchoredPosition: new Vector2(0f, 18f),
            sizeDelta: new Vector2(620f, 40f)
        );

        Button closeButton = UIFactory.CreateButton(
            parent: transform,
            name: "CloseButton",
            label: "X",
            anchorMin: new Vector2(1f, 1f),
            anchorMax: new Vector2(1f, 1f),
            pivot: new Vector2(1f, 1f),
            anchoredPosition: new Vector2(-10f, -10f),
            sizeDelta: new Vector2(36f, 36f)
        );

        closeButton.onClick.AddListener(Close);
    }

    private void ClearChildren()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);
    }
}