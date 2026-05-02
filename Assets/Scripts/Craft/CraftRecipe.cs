using System;
using UnityEngine;

/// <summary>
/// Описание рецепта: комната + два входных элемента + результат.
/// </summary>
[Serializable]
public sealed class CraftRecipe
{
    [SerializeField] private string roomId;
    [SerializeField] private InventoryItemType inputA;
    [SerializeField] private InventoryItemType inputB;
    [SerializeField] private InventoryItemType resultId;

    public string RoomId
    {
        get { return roomId; }
    }

    public InventoryItemType InputA
    {
        get { return inputA; }
    }

    public InventoryItemType InputB
    {
        get { return inputB; }
    }

    public InventoryItemType ResultId
    {
        get { return resultId; }
    }

    public CraftRecipe(string roomId, InventoryItemType inputA, InventoryItemType inputB, InventoryItemType resultId)
    {
        this.roomId = roomId;
        this.inputA = inputA;
        this.inputB = inputB;
        this.resultId = resultId;
    }
}