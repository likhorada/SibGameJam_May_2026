using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Инвентарь на 5 слотов. Один слот хранит только один элемент.
/// </summary>
public sealed class Inventory : MonoBehaviour
{
    public static Inventory Instance { get; private set; }

    public const int SlotCount = 5;

    private readonly InventorySlotData[] slots = new InventorySlotData[SlotCount];

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

    public bool AddElement(ElementKind elementId)
    {
        Initialize();

        int emptyIndex = FindFirstEmptySlot();

        if (emptyIndex < 0)
        {
            Debug.Log("Inventory is full");
            return false;
        }

        slots[emptyIndex].Set(elementId);
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

    public bool TrySetSlot(int index, ElementKind elementId)
    {
        Initialize();

        if (!IsValidIndex(index))
            return false;

        if (!slots[index].IsEmpty)
            return false;

        slots[index].Set(elementId);
        NotifyChanged();
        return true;
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