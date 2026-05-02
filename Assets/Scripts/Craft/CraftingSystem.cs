using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Крафт через словарь рецептов. Никаких ветвлений по конкретным комбинациям.
/// </summary>
public sealed class CraftingSystem : MonoBehaviour
{
    public static CraftingSystem Instance { get; private set; }

    [SerializeField] private List<CraftRecipe> recipes = new List<CraftRecipe>();

    private readonly Dictionary<CraftKey, CraftResult> recipeMap = new Dictionary<CraftKey, CraftResult>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        RebuildRecipeMap();
    }

    public void ConfigureRecipes(IEnumerable<CraftRecipe> newRecipes)
    {
        recipes.Clear();
        recipes.AddRange(newRecipes);
        RebuildRecipeMap();
    }

    public bool TryCraft(string roomId, string inputA, string inputB, out CraftResult result)
    {
        CraftKey key = new CraftKey(roomId, inputA, inputB);
        return recipeMap.TryGetValue(key, out result);
    }

    private void RebuildRecipeMap()
    {
        recipeMap.Clear();

        for (int i = 0; i < recipes.Count; i++)
        {
            CraftRecipe recipe = recipes[i];
            CraftKey key = new CraftKey(recipe.RoomId, recipe.InputA, recipe.InputB);
            CraftResult result = new CraftResult(recipe.ResultId, recipe.ResultName);

            recipeMap[key] = result;
        }
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

/// <summary>
/// Результат успешного крафта.
/// </summary>
public readonly struct CraftResult
{
    public string Id { get; }
    public string DisplayName { get; }

    public CraftResult(string id, string displayName)
    {
        Id = id;
        DisplayName = displayName;
    }
}