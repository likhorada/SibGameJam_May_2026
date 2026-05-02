using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI стола крафта.
/// При закрытии возвращает обычные элементы в инвентарь.
/// Элементы с DiscardOnTableClose исчезают.
/// </summary>
public sealed class CraftingPanelUI : MonoBehaviour
{
    private Canvas rootCanvas;
    private RectTransform tableArea;
    private Text titleText;
    private Text resultText;

    private string currentTableId;
    private string currentRoomId;

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

    public void Open(string tableId, string roomId)
    {
        currentTableId = tableId;
        currentRoomId = roomId;

        gameObject.SetActive(true);

        titleText.text = "Craft Table / " + currentTableId + " / " + currentRoomId;
        resultText.text = "Drag elements to the table";
    }

    public void Close()
    {
        bool returned = ReturnTableItemsOnClose();

        if (!returned)
        {
            resultText.text = "Inventory is full";
            return;
        }

        gameObject.SetActive(false);
    }

    public void CreateTableItem(
        ElementDefinition element,
        Vector2 tablePosition,
        bool shouldReturnToInventoryOnClose)
    {
        if (element == null)
            return;

        GameObject itemObject = new GameObject("TableItem_" + element.Id);
        itemObject.transform.SetParent(tableArea, false);

        RectTransform rectTransform = itemObject.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(110f, 80f);
        rectTransform.anchoredPosition = tablePosition;

        Image image = itemObject.AddComponent<Image>();
        image.color = element.FallbackColor;
        image.raycastTarget = true;

        if (element.Icon != null)
            image.sprite = element.Icon;

        TableItemUI tableItem = itemObject.AddComponent<TableItemUI>();
        tableItem.Configure(
            rootCanvas,
            this,
            tableArea,
            element,
            shouldReturnToInventoryOnClose
        );

        tableItems.Add(tableItem);
    }

    public bool TryCraftTableItems(TableItemUI first, TableItemUI second, Vector2 resultPosition)
    {
        if (CraftingSystem.Instance == null)
        {
            Debug.LogError("CraftingPanelUI: CraftingSystem instance not found");
            return false;
        }

        bool success = CraftingSystem.Instance.TryCraft(
            currentRoomId,
            first.Element,
            second.Element,
            out ElementDefinition result
        );

        if (!success)
        {
            resultText.text = "No recipe";
            return false;
        }

        first.DestroySelf();
        second.DestroySelf();

        // Результат крафта считается обычным полученным предметом.
        // Но если сам результат помечен как DiscardOnTableClose, он тоже исчезнет при закрытии.
        CreateTableItem(
            result,
            resultPosition,
            shouldReturnToInventoryOnClose: true
        );

        resultText.text = "Created: " + result.DisplayName;

        return true;
    }

    public void UnregisterTableItem(TableItemUI item)
    {
        tableItems.Remove(item);
    }

    private bool ReturnTableItemsOnClose()
    {
        if (Inventory.Instance == null)
        {
            Debug.LogError("CraftingPanelUI: Inventory instance not found");
            return false;
        }

        int requiredFreeSlots = CountItemsThatNeedInventoryReturn();

        if (Inventory.Instance.FreeSlotCount() < requiredFreeSlots)
            return false;

        List<TableItemUI> itemsCopy = new List<TableItemUI>(tableItems);

        for (int i = 0; i < itemsCopy.Count; i++)
        {
            TableItemUI item = itemsCopy[i];

            if (item == null)
                continue;

            bool shouldReturn =
                item.ShouldReturnToInventoryOnClose
                && item.Element != null
                && !item.Element.DiscardOnTableClose;

            if (shouldReturn)
            {
                bool added = Inventory.Instance.AddElement(item.Element);

                if (!added)
                    return false;
            }

            // Если это Clay или другой элемент с DiscardOnTableClose,
            // он просто уничтожается и не возвращается в инвентарь.
            item.DestroySelf();
        }

        tableItems.Clear();
        return true;
    }

    private int CountItemsThatNeedInventoryReturn()
    {
        int count = 0;

        for (int i = 0; i < tableItems.Count; i++)
        {
            TableItemUI item = tableItems[i];

            if (item == null)
                continue;

            if (item.Element == null)
                continue;

            if (item.Element.DiscardOnTableClose)
                continue;

            if (item.ShouldReturnToInventoryOnClose)
                count++;
        }

        return count;
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

        GameObject tableObject = new GameObject("FreeTableArea");
        tableObject.transform.SetParent(transform, false);

        RectTransform tableRect = tableObject.AddComponent<RectTransform>();
        tableRect.anchorMin = new Vector2(0.5f, 0.5f);
        tableRect.anchorMax = new Vector2(0.5f, 0.5f);
        tableRect.pivot = new Vector2(0.5f, 0.5f);
        tableRect.anchoredPosition = Vector2.zero;
        tableRect.sizeDelta = new Vector2(620f, 320f);

        Image tableImage = tableObject.AddComponent<Image>();
        tableImage.color = new Color(0.08f, 0.08f, 0.08f, 0.95f);
        tableImage.raycastTarget = true;

        tableArea = tableObject.transform as RectTransform;

        TableDropArea dropArea = tableObject.AddComponent<TableDropArea>();
        dropArea.Configure(this);

        resultText = UIFactory.CreateText(
            parent: transform,
            name: "ResultText",
            value: "Drag elements to the table",
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