using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Инвентарь игрока.
/// 4 обычных слота + отдельный вечный слот глины в UI.
/// Вечная глина не хранится здесь как обычный слот.
/// </summary>
public sealed class Inventory : MonoBehaviour
{
    public static Inventory Instance { get; private set; }

    public const int NormalSlotCount = 4;

    private readonly InventorySlotData[] slots = new InventorySlotData[NormalSlotCount];

    private bool initialized;

    public event Action Changed;

    private void Awake()
    {
        Initialize();
    }

    private void OnEnable()
    {
        Initialize();
    }

    public void Initialize()
    {
        if (initialized)
            return;

        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Inventory: duplicate instance found, disabling duplicate on " + gameObject.name);
            enabled = false;
            return;
        }

        Instance = this;

        for (int i = 0; i < slots.Length; i++)
            slots[i] = new InventorySlotData();

        initialized = true;
        Debug.Log("Inventory: initialized");
    }

    public InventorySlotData GetSlot(int index)
    {
        if (!IsValidIndex(index))
            return null;

        return slots[index];
    }

    public IReadOnlyList<InventorySlotData> GetSlots()
    {
        return slots;
    }

    public bool AddElement(ElementDefinition element)
    {
        Initialize();

        if (element == null)
        {
            Debug.LogError("Inventory: cannot add null element");
            return false;
        }

        int emptyIndex = FindFirstEmptySlot();

        if (emptyIndex < 0)
        {
            Debug.Log("Inventory is full");
            return false;
        }

        slots[emptyIndex].Set(element);
        NotifyChanged();
        return true;
    }

    public bool TrySetSlot(int index, ElementDefinition element)
    {
        Initialize();

        if (!IsValidIndex(index))
            return false;

        if (!slots[index].IsEmpty)
            return false;

        if (element == null)
            return false;

        slots[index].Set(element);
        NotifyChanged();
        return true;
    }

    public bool TryClearSlot(int index)
    {
        Initialize();

        if (!IsValidIndex(index))
            return false;

        if (slots[index].IsEmpty)
            return false;

        slots[index].Clear();
        NotifyChanged();
        return true;
    }

    public bool HasElement(ElementDefinition element)
    {
        Initialize();

        if (element == null)
            return false;

        for (int i = 0; i < slots.Length; i++)
        {
            if (!slots[i].IsEmpty && slots[i].Element == element)
                return true;
        }

        return false;
    }

    public bool TryConsumeElement(ElementDefinition element)
    {
        Initialize();

        if (element == null)
            return false;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].IsEmpty)
                continue;

            if (slots[i].Element != element)
                continue;

            slots[i].Clear();
            NotifyChanged();
            return true;
        }

        return false;
    }

    public void ClearAllNormalSlots()
    {
        Initialize();

        for (int i = 0; i < slots.Length; i++)
            slots[i].Clear();

        NotifyChanged();

        Debug.Log("Inventory: all normal slots cleared");
    }

    public bool HasFreeSlot()
    {
        Initialize();
        return FindFirstEmptySlot() >= 0;
    }

    public int FreeSlotCount()
    {
        Initialize();

        int count = 0;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].IsEmpty)
                count++;
        }

        return count;
    }

    private int FindFirstEmptySlot()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i].IsEmpty)
                return i;
        }

        return -1;
    }

    private bool IsValidIndex(int index)
    {
        return index >= 0 && index < slots.Length;
    }

    private void NotifyChanged()
    {
        Changed?.Invoke();
    }
}