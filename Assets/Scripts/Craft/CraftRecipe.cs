using System;
using UnityEngine;

/// <summary>
/// Рецепт: в конкретной комнате два элемента дают один результат.
/// Порядок входных элементов не важен.
/// </summary>
[Serializable]
public sealed class CraftRecipe
{
    [SerializeField] private string roomId;
    [SerializeField] private ElementDefinition inputA;
    [SerializeField] private ElementDefinition inputB;
    [SerializeField] private ElementDefinition result;

    public string RoomId
    {
        get { return roomId; }
    }

    public ElementDefinition InputA
    {
        get { return inputA; }
    }

    public ElementDefinition InputB
    {
        get { return inputB; }
    }

    public ElementDefinition Result
    {
        get { return result; }
    }
}