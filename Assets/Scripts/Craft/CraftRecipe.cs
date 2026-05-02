using System;
using UnityEngine;

/// <summary>
/// Описание рецепта: комната + два входных элемента + результат.
/// </summary>
[Serializable]
public sealed class CraftRecipe
{
    [SerializeField] private string roomId;
    [SerializeField] private ElementKind inputA;
    [SerializeField] private ElementKind inputB;
    [SerializeField] private ElementKind resultId;

    public string RoomId
    {
        get { return roomId; }
    }

    public ElementKind InputA
    {
        get { return inputA; }
    }

    public ElementKind InputB
    {
        get { return inputB; }
    }

    public ElementKind ResultId
    {
        get { return resultId; }
    }

    public CraftRecipe(string roomId, ElementKind inputA, ElementKind inputB, ElementKind resultId)
    {
        this.roomId = roomId;
        this.inputA = inputA;
        this.inputB = inputB;
        this.resultId = resultId;
    }
}