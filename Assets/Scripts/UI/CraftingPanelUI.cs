using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum TableItemBackgroundMode
{
    None,
    ElementFallbackColor,
    CustomStyle
}

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

    public static int LastEscapeCloseFrame { get; private set; } = -1;

    public static event Action<string, string, ElementDefinition> ElementCrafted;

    [Header("Panel Layout")]
    [SerializeField] private Vector2 panelSize = new Vector2(1050f, 650f);
    [SerializeField] private Vector2 panelPosition = new Vector2(0f, 40f);

    [Header("Panel Visual")]
    [SerializeField] private UIImageStyle panelStyle =
        UIImageStyle.Create(new Color(0.055f, 0.045f, 0.035f, 0.92f), true);

    [Header("Table Area Layout")]
    [SerializeField] private Vector2 tableAreaSize = new Vector2(900f, 470f);
    [SerializeField] private Vector2 tableAreaPosition = new Vector2(0f, 0f);

    [Header("Table Item Layout")]
    [SerializeField] private Vector2 tableItemSize = new Vector2(180f, 142f);
    [SerializeField] private Vector2 tableItemIconSize = new Vector2(112f, 112f);
    [SerializeField] private Vector2 tableItemIconPosition = new Vector2(0f, 30f);
    [SerializeField] private int tableItemLabelFontSize = 16;
    [SerializeField] private float tableItemLabelHeight = 34f;

    [Header("Table Item Background")]
    [Tooltip("None keeps table items visually transparent. ElementFallbackColor uses Element Definition color. CustomStyle uses Table Item Style.")]
    [SerializeField] private TableItemBackgroundMode tableItemBackgroundMode = TableItemBackgroundMode.None;
    [Tooltip("Used only when Table Item Background Mode is CustomStyle.")]
    [SerializeField] private UIImageStyle tableItemStyle =
        UIImageStyle.Create(new Color(0.16f, 0.13f, 0.1f, 0.82f), true);

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
    [SerializeField] private Sprite tableAreaSprite;

    private Canvas rootCanvas;
    private RectTransform tableArea;
    private Text titleText;
    private Text resultText;

    private string currentTableId;
    private string currentRoomId;

    private bool isBuilt;
    private bool isOpen;

    private readonly List<TableItemUI> tableItems = new List<TableItemUI>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        openPanelCount = 0;
        LastEscapeCloseFrame = -1;
        ElementCrafted = null;
    }

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
        {
            LastEscapeCloseFrame = Time.frameCount;
            Close();
        }
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

        GameAudio.Play(GameSoundId.TableOpen);
    }

    public void Close()
    {
        bool returned = ReturnTableItemsOnClose();

        if (!returned)
        {
            if (resultText != null)
                resultText.text = "Inventory is full";

            GameAudio.Play(GameSoundId.InventoryFull);
            return;
        }

        gameObject.SetActive(false);
        GameAudio.Play(GameSoundId.TableClose);
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

        Image backgroundImage = itemObject.AddComponent<Image>();
        ApplyTableItemBackground(backgroundImage, element);

        GameObject iconObject = new GameObject("Icon");
        iconObject.transform.SetParent(itemObject.transform, false);

        RectTransform iconRect = iconObject.AddComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0.5f, 0.5f);
        iconRect.anchorMax = new Vector2(0.5f, 0.5f);
        iconRect.pivot = new Vector2(0.5f, 0.5f);
        iconRect.anchoredPosition = tableItemIconPosition;
        iconRect.sizeDelta = tableItemIconSize;

        Image iconImage = iconObject.AddComponent<Image>();
        iconImage.preserveAspect = true;
        iconImage.raycastTarget = false;

        if (element.Icon != null)
        {
            iconImage.sprite = element.Icon;
            iconImage.color = Color.white;
        }
        else
        {
            iconImage.sprite = null;
            iconImage.color = element.FallbackColor;
        }

        Text label = UIFactory.CreateText(
            parent: itemObject.transform,
            name: "Label",
            value: element.DisplayName,
            fontSize: tableItemLabelFontSize,
            alignment: TextAnchor.LowerCenter,
            anchorMin: new Vector2(0f, 0f),
            anchorMax: new Vector2(1f, 0f),
            pivot: new Vector2(0.5f, 0f),
            anchoredPosition: new Vector2(0f, 7f),
            sizeDelta: new Vector2(-12f, tableItemLabelHeight)
        );

        label.raycastTarget = false;

        TableItemUI tableItem = itemObject.AddComponent<TableItemUI>();
        tableItem.Configure(
            rootCanvas,
            this,
            tableArea,
            element,
            shouldReturnToInventoryOnClose,
            iconImage,
            label
        );

        tableItems.Add(tableItem);
    }

    private void ApplyTableItemBackground(Image image, ElementDefinition element)
    {
        if (image == null)
            return;

        if (ElementUIBackgroundUtility.TryApplyPersonalBackground(element, image, true))
            return;

        if (tableItemBackgroundMode == TableItemBackgroundMode.CustomStyle)
        {
            if (tableItemStyle == null)
                tableItemStyle = UIImageStyle.Create(new Color(0.16f, 0.13f, 0.1f, 0.82f), true);

            ElementUIBackgroundUtility.ApplyStyle(tableItemStyle, image, true);
            return;
        }

        if (tableItemBackgroundMode == TableItemBackgroundMode.ElementFallbackColor && element != null)
        {
            ElementUIBackgroundUtility.ApplyFallbackColor(element, image, true);
            return;
        }

        ElementUIBackgroundUtility.ApplyTransparent(image, true);
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

            GameAudio.Play(GameSoundId.CraftFail);
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

        NotifyElementCrafted(result);
        GameAudio.Play(GameSoundId.CraftSuccess);

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
            GameAudio.Play(GameSoundId.CraftFail);
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

            GameAudio.Play(GameSoundId.CraftFail);

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

        NotifyElementCrafted(result);
        GameAudio.Play(GameSoundId.CraftSuccess);

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

        Image panelImage = GetComponent<Image>();

        if (panelImage == null)
            panelImage = gameObject.AddComponent<Image>();

        if (panelStyle == null)
            panelStyle = UIImageStyle.Create(new Color(0.055f, 0.045f, 0.035f, 0.92f), true);

        panelStyle.ApplyTo(panelImage);
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

    private void NotifyElementCrafted(ElementDefinition result)
    {
        ElementCrafted?.Invoke(currentTableId, currentRoomId, result);
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
        tableImage.sprite = tableAreaSprite;
        tableImage.type = tableAreaSprite == null ? Image.Type.Simple : Image.Type.Sliced;
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
