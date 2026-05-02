using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Система крафта через словарь.
/// Никаких if/else по комбинациям.
/// </summary>
public sealed class CraftingSystem : MonoBehaviour
{
    public static CraftingSystem Instance { get; private set; }

    [SerializeField] private CraftRecipeDatabase recipeDatabase;

    private readonly Dictionary<CraftKey, ElementDefinition> recipeMap =
        new Dictionary<CraftKey, ElementDefinition>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("CraftingSystem: duplicate instance found, disabling duplicate");
            enabled = false;
            return;
        }

        Instance = this;
        RebuildRecipeMap();
    }

    public void Configure(CraftRecipeDatabase database)
    {
        recipeDatabase = database;
        RebuildRecipeMap();
    }

    public bool TryCraft(
        string roomId,
        ElementDefinition first,
        ElementDefinition second,
        out ElementDefinition result)
    {
        result = null;

        if (string.IsNullOrEmpty(roomId))
            return false;

        if (first == null || second == null)
            return false;

        CraftKey key = new CraftKey(roomId, first.Id, second.Id);
        return recipeMap.TryGetValue(key, out result);
    }

    private void RebuildRecipeMap()
    {
        recipeMap.Clear();

        if (recipeDatabase == null)
            return;

        IReadOnlyList<CraftRecipe> recipes = recipeDatabase.Recipes;

        for (int i = 0; i < recipes.Count; i++)
        {
            CraftRecipe recipe = recipes[i];

            if (recipe == null)
                continue;

            if (recipe.InputA == null || recipe.InputB == null || recipe.Result == null)
                continue;

            if (string.IsNullOrEmpty(recipe.RoomId))
                continue;

            CraftKey key = new CraftKey(
                recipe.RoomId,
                recipe.InputA.Id,
                recipe.InputB.Id
            );

            recipeMap[key] = recipe.Result;
        }

        Debug.Log("CraftingSystem: recipes loaded = " + recipeMap.Count);
    }

    private readonly struct CraftKey : IEquatable<CraftKey>
    {
        private readonly string roomId;
        private readonly string firstElementId;
        private readonly string secondElementId;

        public CraftKey(string roomId, string inputA, string inputB)
        {
            this.roomId = roomId;

            int compare = string.CompareOrdinal(inputA, inputB);

            if (compare <= 0)
            {
                firstElementId = inputA;
                secondElementId = inputB;
            }
            else
            {
                firstElementId = inputB;
                secondElementId = inputA;
            }
        }

        public bool Equals(CraftKey other)
        {
            return roomId == other.roomId
                && firstElementId == other.firstElementId
                && secondElementId == other.secondElementId;
        }

        public override bool Equals(object obj)
        {
            return obj is CraftKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + (roomId == null ? 0 : roomId.GetHashCode());
                hash = hash * 31 + (firstElementId == null ? 0 : firstElementId.GetHashCode());
                hash = hash * 31 + (secondElementId == null ? 0 : secondElementId.GetHashCode());
                return hash;
            }
        }
    }
}