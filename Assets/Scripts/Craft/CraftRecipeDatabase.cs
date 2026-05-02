using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// База рецептов крафта.
/// Хранится как ScriptableObject, чтобы рецепты добавлялись через Inspector.
/// </summary>
[CreateAssetMenu(
    fileName = "CraftRecipeDatabase",
    menuName = "Golem Craft/Craft Recipe Database"
)]
public sealed class CraftRecipeDatabase : ScriptableObject
{
    [SerializeField] private List<CraftRecipe> recipes = new List<CraftRecipe>();

    public IReadOnlyList<CraftRecipe> Recipes
    {
        get { return recipes; }
    }
}