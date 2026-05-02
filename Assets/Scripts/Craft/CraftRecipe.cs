using System;
using UnityEngine;

/// <summary>
/// Описание рецепта: комната + два входных элемента + результат.
/// </summary>
[Serializable]
public sealed class CraftRecipe
{
    [SerializeField] private string roomId;
    [SerializeField] private string inputA;
    [SerializeField] private string inputB;
    [SerializeField] private string resultId;
    [SerializeField] private string resultName;

    public string RoomId
    {
        get { return roomId; }
    }

    public string InputA
    {
        get { return inputA; }
    }

    public string InputB
    {
        get { return inputB; }
    }

    public string ResultId
    {
        get { return resultId; }
    }

    public string ResultName
    {
        get { return resultName; }
    }

    public CraftRecipe(string roomId, string inputA, string inputB, string resultId, string resultName)
    {
        this.roomId = roomId;
        this.inputA = inputA;
        this.inputB = inputB;
        this.resultId = resultId;
        this.resultName = resultName;
    }
}