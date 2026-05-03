using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI стола крафта.
/// При закрытии возвращает обычные элементы в инвентарь.
/// Элементы с DiscardOnTableClose исчезают.
/// Размеры панели и поля настраиваются в Inspector.
/// </summary>
public sealed class CraftingPanelUI : MonoBehaviour
{
    private static int openPanelCount;

    public static bool HasOpenPanel
    {
        get { return openPanelCount > 0; }
    }

    [Header("Panel Layout")]
    [SerializeField] private Vector2 panelSize = new Vector2(1050f, 650f);
    [SerializeField] private Vector2 panelPosition = new Vector2(0f, 40f);

    [Header("Table Area Layout")]
    [SerializeField] private Vector2 tableAreaSize = new Vector2(900f, 470f);
    [SerializeField] private Vector2 tableAreaPosition = new Vector2(0f, 0f);

    [Header("Table Item Layout")]
    [SerializeField] private Vector2 tableItemSize = new Vector2(150f, 110f);

    [Header("Text Layout")]
    [SerializeField] private Vector2 titlePosition = new Vector2(0f, -18f);
    [SerializeField] private Vector2 titleSize = new Vector2(900f, 40f);
    [SerializeField] private Vector2 resultTextPosition = new Vector2(0f, 20f);
    [SerializeField] private Vector2 resultTextSize = new Vector2(900f, 40f);

    [Header("Close Button Layout")]
    [SerializeField] private Vector2 closeButtonPosition = new Vector2(-12f, -12f);
    [SerializeField] private Vector2 closeButtonSize = new Vector2(42f, 42f);

    [Header("Colors")]
    [SerializeField] private Color tableAreaColor = new Color(0.08f, 0.08f, 0.08f, 0.95f);

    private Canvas rootCanvas;
    private RectTransform tableArea;
    private Text titleText;
    private Text resultText;

    private string currentTableId;
    private string currentRoomId;

    private bool isBuilt;
    private bool isOpen;

    private readonly List<TableItemUI> tableItems = new List<TableItemUI>();

    public void Configure(Canvas canvas)
    {
        rootCanvas = canvas;

        ForcePanelRect();

        if (!isBuilt)
            BuildUI();
    }

    private void Update()
    {
        if (gameObject.activeSelf && Input.GetKeyDown(KeyCode.Escape))
            Close();
    }

    private void OnDisable()
    {
        MarkClosed();
    }

    public void Open(string tableId, string roomId)
    {
        currentTableId = tableId;
        currentRoomId = roomId;

        ForcePanelRect();
        gameObject.SetActive(true);
        MarkOpened();

        if (titleText != null)
            titleText.text = "Craft Table / " + currentTableId + " / " + currentRoomId;

        if (resultText != null)
            resultText.text = "Drag elements to the table";
    }

    public void Close()
    {
        bool returned = ReturnTableItemsOnClose();

        if (!returned)
        {
            if (resultText != null)
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
        rectTransform.sizeDelta = tableItemSize;
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
            if (resultText != null)
                resultText.text = "No recipe";

            return false;
        }

        first.DestroySelf();
        second.DestroySelf();

        CreateTableItem(
            result,
            resultPosition,
            shouldReturnToInventoryOnClose: true
        );

        if (resultText != null)
            resultText.text = "Created: " + result.DisplayName;

        return true;
    }

    public bool TryCraftIncomingElement(
        ElementDefinition incomingElement,
        TableItemUI tableItem,
        Vector2 resultPosition,
        bool consumeInventorySlot,
        int inventorySlotIndex)
    {
        if (CraftingSystem.Instance == null)
        {
            Debug.LogError("CraftingPanelUI: CraftingSystem instance not found");
            return false;
        }

        if (incomingElement == null || tableItem == null || tableItem.Element == null)
            return false;

        bool success = CraftingSystem.Instance.TryCraft(
            currentRoomId,
            incomingElement,
            tableItem.Element,
            out ElementDefinition result
        );

        if (!success)
        {
            if (resultText != null)
                resultText.text = "No recipe";

            return false;
        }

        if (consumeInventorySlot)
        {
            if (Inventory.Instance == null)
            {
                Debug.LogError("CraftingPanelUI: Inventory instance not found");
                return false;
            }

            bool removedFromInventory =
                Inventory.Instance.TryClearSlot(inventorySlotIndex);

            if (!removedFromInventory)
                return false;
        }

        tableItem.DestroySelf();

        CreateTableItem(
            result,
            resultPosition,
            shouldReturnToInventoryOnClose: true
        );

        if (resultText != null)
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

    private void ForcePanelRect()
    {
        RectTransform rectTransform = transform as RectTransform;

        if (rectTransform == null)
            return;

        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = panelPosition;
        rectTransform.sizeDelta = panelSize;
    }

    private void MarkOpened()
    {
        if (isOpen)
            return;

        isOpen = true;
        openPanelCount++;
    }

    private void MarkClosed()
    {
        if (!isOpen)
            return;

        isOpen = false;
        openPanelCount = Mathf.Max(0, openPanelCount - 1);
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
            anchoredPosition: titlePosition,
            sizeDelta: titleSize
        );

        GameObject tableObject = new GameObject("FreeTableArea");
        tableObject.transform.SetParent(transform, false);

        RectTransform tableRect = tableObject.AddComponent<RectTransform>();
        tableRect.anchorMin = new Vector2(0.5f, 0.5f);
        tableRect.anchorMax = new Vector2(0.5f, 0.5f);
        tableRect.pivot = new Vector2(0.5f, 0.5f);
        tableRect.anchoredPosition = tableAreaPosition;
        tableRect.sizeDelta = tableAreaSize;

        Image tableImage = tableObject.AddComponent<Image>();
        tableImage.color = tableAreaColor;
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
            anchoredPosition: resultTextPosition,
            sizeDelta: resultTextSize
        );

        Button closeButton = UIFactory.CreateButton(
            parent: transform,
            name: "CloseButton",
            label: "X",
            anchorMin: new Vector2(1f, 1f),
            anchorMax: new Vector2(1f, 1f),
            pivot: new Vector2(1f, 1f),
            anchoredPosition: closeButtonPosition,
            sizeDelta: closeButtonSize
        );

        closeButton.onClick.AddListener(Close);
    }

    private void ClearChildren()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);
    }
}
