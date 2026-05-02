using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Нижняя панель инвентаря на 5 видимых слотов.
/// Сама выставляет RectTransform, чтобы панель не уезжала за экран.
/// </summary>
public sealed class InventoryUI : MonoBehaviour
{
    private Canvas rootCanvas;
    private bool configured;

    private readonly InventorySlotUI[] slotUIs = new InventorySlotUI[Inventory.SlotCount];

    public void Configure(Canvas canvas)
    {
        rootCanvas = canvas;
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
        rectTransform.sizeDelta = new Vector2(0f, 125f);
        rectTransform.offsetMin = new Vector2(0f, 0f);
        rectTransform.offsetMax = new Vector2(0f, 125f);
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

        float startX = 150f;
        float spacing = 170f;

        for (int i = 0; i < Inventory.SlotCount; i++)
        {
            Vector2 position = new Vector2(startX + spacing * i, 0f);
            CreateSlot(i, position);
        }
    }

    private void CreateSlot(int index, Vector2 position)
    {
        GameObject slotObject = new GameObject("InventorySlot_" + index);
        slotObject.transform.SetParent(transform, false);

        RectTransform rectTransform = slotObject.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0f, 0.5f);
        rectTransform.anchorMax = new Vector2(0f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = new Vector2(150f, 70f);

        Image image = slotObject.AddComponent<Image>();
        image.color = new Color(0.18f, 0.18f, 0.18f, 1f);
        image.raycastTarget = true;

        Text label = UIFactory.CreateText(
            parent: slotObject.transform,
            name: "Label",
            value: "Empty",
            fontSize: 15,
            alignment: TextAnchor.MiddleCenter,
            anchorMin: Vector2.zero,
            anchorMax: Vector2.one,
            pivot: new Vector2(0.5f, 0.5f),
            anchoredPosition: Vector2.zero,
            sizeDelta: Vector2.zero
        );

        label.raycastTarget = false;

        InventorySlotUI slotUI = slotObject.AddComponent<InventorySlotUI>();
        slotUI.Configure(index, rootCanvas, label, image);

        slotUIs[index] = slotUI;
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
        {
            Debug.LogWarning("InventoryUI: Inventory.Instance is null during Refresh");
            return;
        }

        for (int i = 0; i < slotUIs.Length; i++)
        {
            if (slotUIs[i] == null)
                continue;

            InventorySlotData data = Inventory.Instance.GetSlot(i);
            slotUIs[i].SetData(data);
        }

        Debug.Log("InventoryUI: refreshed");
    }

    private void ClearChildren()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);
    }
}