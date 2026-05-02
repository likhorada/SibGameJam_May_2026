using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Нижняя панель инвентаря.
/// 4 обычных слота + 1 вечный слот глины.
/// </summary>
public sealed class InventoryUI : MonoBehaviour
{
    private const int PermanentClaySlotIndex = Inventory.NormalSlotCount;

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
        rectTransform.sizeDelta = new Vector2(0f, 120f);
        rectTransform.offsetMin = new Vector2(0f, 0f);
        rectTransform.offsetMax = new Vector2(0f, 120f);
    }

    private void BuildSlots()
    {
        ClearChildren();

        UIFactory.CreateText(
            parent: transform,
            name: "InventoryTitle",
            value: "Inventory",
            fontSize: 18,
            alignment: TextAnchor.MiddleLeft,
            anchorMin: new Vector2(0f, 0.5f),
            anchorMax: new Vector2(0f, 0.5f),
            pivot: new Vector2(0f, 0.5f),
            anchoredPosition: new Vector2(18f, 0f),
            sizeDelta: new Vector2(120f, 70f)
        );

        float startX = 160f;
        float spacing = 140f;

        for (int i = 0; i < Inventory.NormalSlotCount; i++)
        {
            Vector2 position = new Vector2(startX + spacing * i, 0f);
            CreateNormalSlot(i, position);
        }

        Vector2 clayPosition = new Vector2(startX + spacing * PermanentClaySlotIndex, 0f);
        CreatePermanentClaySlot(clayPosition);
    }

    private void CreateNormalSlot(int index, Vector2 position)
    {
        InventorySlotUI slotUI = CreateSlotBase(
            name: "InventorySlot_" + index,
            position: position,
            out Image background,
            out Image icon,
            out Text label
        );

        slotUI.ConfigureNormalSlot(index, rootCanvas, background, icon, label);
        slotUIs[index] = slotUI;
    }

    private void CreatePermanentClaySlot(Vector2 position)
    {
        InventorySlotUI slotUI = CreateSlotBase(
            name: "PermanentClaySlot",
            position: position,
            out Image background,
            out Image icon,
            out Text label
        );

        slotUI.ConfigurePermanentElementSlot(
            rootCanvas,
            background,
            icon,
            label,
            clayElement
        );

        slotUIs[PermanentClaySlotIndex] = slotUI;
    }

    private InventorySlotUI CreateSlotBase(
        string name,
        Vector2 position,
        out Image background,
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
        rectTransform.sizeDelta = new Vector2(110f, 90f);

        background = slotObject.AddComponent<Image>();
        background.color = new Color(0.18f, 0.18f, 0.18f, 1f);
        background.raycastTarget = true;

        GameObject iconObject = new GameObject("Icon");
        iconObject.transform.SetParent(slotObject.transform, false);

        RectTransform iconRect = iconObject.AddComponent<RectTransform>();
        iconRect.anchorMin = new Vector2(0.5f, 0.5f);
        iconRect.anchorMax = new Vector2(0.5f, 0.5f);
        iconRect.pivot = new Vector2(0.5f, 0.5f);
        iconRect.anchoredPosition = new Vector2(0f, 10f);
        iconRect.sizeDelta = new Vector2(48f, 48f);

        icon = iconObject.AddComponent<Image>();
        icon.raycastTarget = false;
        icon.enabled = false;

        label = UIFactory.CreateText(
            parent: slotObject.transform,
            name: "Label",
            value: "Empty",
            fontSize: 13,
            alignment: TextAnchor.LowerCenter,
            anchorMin: Vector2.zero,
            anchorMax: Vector2.one,
            pivot: new Vector2(0.5f, 0.5f),
            anchoredPosition: new Vector2(0f, 4f),
            sizeDelta: Vector2.zero
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