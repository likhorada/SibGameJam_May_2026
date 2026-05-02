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

    public bool TryCraft(string roomId, ElementKind inputA, ElementKind inputB, out CraftResult result)
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
            CraftResult result = new CraftResult(recipe.ResultId);

            recipeMap[key] = result;
        }
    }

    private readonly struct CraftKey : IEquatable<CraftKey>
    {
        private readonly string roomId;
        private readonly ElementKind firstElementId;
        private readonly ElementKind secondElementId;

        public CraftKey(string roomId, ElementKind inputA, ElementKind inputB)
        {
            this.roomId = roomId;

            if (inputA <= inputB)
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
                hash = hash * 31 + (int)firstElementId;
                hash = hash * 31 + (int)secondElementId;
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
    public ElementKind ElementId { get; }

    public CraftResult(ElementKind elementId)
    {
        ElementId = elementId;
    }
}