using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Нижняя панель инвентаря.
/// 4 обычных слота + 1 вечный слот глины.
/// </summary>
public sealed class InventoryUI : MonoBehaviour
{
    private const int PermanentClaySlotIndex = Inventory.NormalSlotCount;

    [Header("Panel Visual")]
    [SerializeField] private UIImageStyle panelStyle =
        UIImageStyle.Create(new Color(0.06f, 0.055f, 0.05f, 0.9f), true);
    [SerializeField] private float panelHeight = 134f;

    [Header("Slot Visual")]
    [SerializeField] private UIImageStyle slotStyle =
        UIImageStyle.Create(new Color(0.18f, 0.16f, 0.13f, 0.95f), true);
    [SerializeField] private Vector2 slotSize = new Vector2(124f, 104f);
    [SerializeField] private float slotStartX = 168f;
    [SerializeField] private float slotSpacing = 150f;

    [Header("Element Visual")]
    [SerializeField] private Vector2 iconSize = new Vector2(72f, 72f);
    [SerializeField] private Vector2 iconPosition = new Vector2(0f, 22f);
    [SerializeField] private int labelFontSize = 15;
    [SerializeField] private float labelHeight = 30f;

    [Header("Title")]
    [SerializeField] private int titleFontSize = 18;

    private Canvas rootCanvas;
    private ElementDefinition clayElement;
    private bool configured;

    private readonly InventorySlotUI[] slotUIs =
        new InventorySlotUI[Inventory.NormalSlotCount + 1];

    public void Configure(Canvas canvas, ElementDefinition clayElement)
    {
        rootCanvas = canvas;
        this.clayElement = clayElement;
        configured = true;

        ForcePanelRect();
        BuildSlots();
        SubscribeToInventory();
        Refresh();

        Debug.Log("InventoryUI: configured");
    }

    private void OnEnable()
    {
        if (!configured)
            return;

        SubscribeToInventory();
        Refresh();
    }

    private void OnDisable()
    {
        UnsubscribeFromInventory();
    }

    private void ForcePanelRect()
    {
        RectTransform rectTransform = transform as RectTransform;

        if (rectTransform == null)
            return;

        rectTransform.anchorMin = new Vector2(0f, 0f);
        rectTransform.anchorMax = new Vector2(1f, 0f);
        rectTransform.pivot = new Vector2(0.5f, 0f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = new Vector2(0f, panelHeight);
        rectTransform.offsetMin = new Vector2(0f, 0f);
        rectTransform.offsetMax = new Vector2(0f, panelHeight);

        Image panelImage = GetComponent<Image>();

        if (panelImage == null)
            panelImage = gameObject.AddComponent<Image>();

        if (panelStyle == null)
            panelStyle = UIImageStyle.Create(new Color(0.06f, 0.055f, 0.05f, 0.9f), true);

        panelStyle.ApplyTo(panelImage);
    }

    private void BuildSlots()
    {
        ClearChildren();

        UIFactory.CreateText(
            parent: transform,
            name: "InventoryTitle",
            value: "Inventory",
            fontSize: titleFontSize,
            alignment: TextAnchor.MiddleLeft,
            anchorMin: new Vector2(0f, 0.5f),
            anchorMax: new Vector2(0f, 0.5f),
            pivot: new Vector2(0f, 0.5f),
            anchoredPosition: new Vector2(18f, 0f),
            sizeDelta: new Vector2(120f, 70f)
        );

        for (int i = 0; i < Inventory.NormalSlotCount; i++)
        {
            Vector2 position = new Vector2(slotStartX + slotSpacing * i, 0f);
            CreateNormalSlot(i, position);
        }

        Vector2 clayPosition = new Vector2(slotStartX + slotSpacing * PermanentClaySlotIndex, 0f);
        CreatePermanentClaySlot(clayPosition);
    }

    private void CreateNormalSlot(int index, Vector2 position)
    {
        InventorySlotUI slotUI = CreateSlotBase(
            name: "InventorySlot_" + index,
            position: position,
            out Image background,
            out Image elementBackground,
            out Image icon,
            out Text label
        );

        slotUI.ConfigureNormalSlot(
            index,
            rootCanvas,
            background,
            elementBackground,
            icon,
            label,
            slotSize,
            iconSize,
            iconPosition,
            labelFontSize,
            labelHeight
        );
        slotUIs[index] = slotUI;
    }

    private void CreatePermanentClaySlot(Vector2 position)
    {
        InventorySlotUI slotUI = CreateSlotBase(
            name: "PermanentClaySlot",
            position: position,
            out Image background,
            out Image elementBackground,
            out Image icon,
            out Text label
        );

        slotUI.ConfigurePermanentElementSlot(
            rootCanvas,
            background,
            elementBackground,
            icon,
            label,
            clayElement,
            slotSize,
            iconSize,
            iconPosition,
            labelFontSize,
            labelHeight
        );

        slotUIs[PermanentClaySlotIndex] = slotUI;
    }

    private InventorySlotUI CreateSlotBase(
        string name,
        Vector2 position,
        out Image background,
        out Image elementBackground,
        out Image icon,
        out Text label)
    {
        GameObject slotObject = new GameObject(name);
        slotObject.transform.SetParent(transform, false);

        RectTransform rectTransform = slotObject.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0f, 0.5f);
        rectTransform.anchorMax = new Vector2(0f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = slotSize;

        background = slotObject.AddComponent<Image>();
        if (slotStyle == null)
            slotStyle = UIImageStyle.Create(new Color(0.18f, 0.16f, 0.13f, 0.95f), true);

        slotStyle.ApplyTo(background);

        GameObject elementBackgroundObject = new GameObject("ElementBackground");
        elementBackgroundObject.transform.SetParent(slotObject.transform, false);

        RectTransform elementBackgroundRect = elementBackgroundObject.AddComponent<RectTransform>();
        elementBackgroundRect.anchorMin = Vector2.zero;
        elementBackgroundRect.anchorMax = Vector2.one;
        elementBackgroundRect.pivot = new Vector2(0.5f, 0.5f);
        elementBackgroundRect.anchoredPosition = Vector2.zero;
        elementBackgroundRect.sizeDelta = Vector2.zero;

        elementBackground = elementBackgroundObject.AddComponent<Image>();
        ElementUIBackgroundUtility.ApplyTransparent(elementBackground, false);

        GameObject iconObject = new GameObject("Icon");
        iconObject.transform.SetParent(slotObject.transform, false);

        RectTransform iconRect = iconObject.AddComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0.5f, 0.5f);
        iconRect.anchorMax = new Vector2(0.5f, 0.5f);
        iconRect.pivot = new Vector2(0.5f, 0.5f);
        iconRect.anchoredPosition = iconPosition;
        iconRect.sizeDelta = iconSize;

        icon = iconObject.AddComponent<Image>();
        icon.preserveAspect = true;
        icon.raycastTarget = false;
        icon.enabled = false;

        label = UIFactory.CreateText(
            parent: slotObject.transform,
            name: "Label",
            value: "Empty",
            fontSize: labelFontSize,
            alignment: TextAnchor.LowerCenter,
            anchorMin: new Vector2(0f, 0f),
            anchorMax: new Vector2(1f, 0f),
            pivot: new Vector2(0.5f, 0f),
            anchoredPosition: new Vector2(0f, 6f),
            sizeDelta: new Vector2(-10f, labelHeight)
        );

        label.raycastTarget = false;

        return slotObject.AddComponent<InventorySlotUI>();
    }

    private void SubscribeToInventory()
    {
        if (Inventory.Instance == null)
            return;

        Inventory.Instance.Changed -= Refresh;
        Inventory.Instance.Changed += Refresh;
    }

    private void UnsubscribeFromInventory()
    {
        if (Inventory.Instance == null)
            return;

        Inventory.Instance.Changed -= Refresh;
    }

    private void Refresh()
    {
        if (Inventory.Instance == null)
            return;

        for (int i = 0; i < Inventory.NormalSlotCount; i++)
        {
            if (slotUIs[i] == null)
                continue;

            InventorySlotData data = Inventory.Instance.GetSlot(i);
            slotUIs[i].SetData(data);
        }

        if (slotUIs[PermanentClaySlotIndex] != null && clayElement != null)
        {
            // Вечный слот не обновляется из Inventory, он всегда содержит глину.
        }
    }

    private void ClearChildren()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);
    }
}
