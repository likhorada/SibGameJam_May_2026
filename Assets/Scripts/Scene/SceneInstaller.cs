using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Связывает уже существующие объекты сцены.
/// Ничего не создаёт. Только настраивает зависимости, UI и рецепты.
/// </summary>
public sealed class SceneInstaller : MonoBehaviour
{
    [Header("Systems")]
    [SerializeField] private CraftingSystem craftingSystem;

    [Header("UI")]
    [SerializeField] private Canvas rootCanvas;
    [SerializeField] private InventoryUI inventoryUI;

    [Header("Craft Tables")]
    [SerializeField] private CraftTableBinding[] craftTables;

    [Header("Recipes")]
    [SerializeField] private List<CraftRecipe> recipes = new List<CraftRecipe>
    {
        new CraftRecipe(
            roomId: "room_01",
            inputA: InventoryItemType.Fire,
            inputB: InventoryItemType.Stone,
            resultId: InventoryItemType.KeyCore
        ),

        new CraftRecipe(
            roomId: "room_02",
            inputA: InventoryItemType.Fire,
            inputB: InventoryItemType.Stone,
            resultId: InventoryItemType.RoomTwoResult
        )
    };

    private void Start()
    {
        Install();
    }

    private void Install()
    {
        if (craftingSystem == null)
        {
            Debug.LogError("SceneInstaller: CraftingSystem is not assigned");
            return;
        }

        if (rootCanvas == null)
        {
            Debug.LogError("SceneInstaller: Root Canvas is not assigned");
            return;
        }

        if (inventoryUI == null)
        {
            Debug.LogError("SceneInstaller: InventoryUI is not assigned");
            return;
        }

        if (Inventory.Instance == null)
        {
            Debug.LogError("SceneInstaller: Inventory instance not found. Check GameSystems/Inventory object");
            return;
        }

        if (CraftingSystem.Instance == null)
        {
            Debug.LogError("SceneInstaller: CraftingSystem instance not found. Check GameSystems/CraftingSystem object");
            return;
        }

        craftingSystem.ConfigureRecipes(recipes);

        inventoryUI.Configure(rootCanvas);

        InstallCraftTables();

        Debug.Log("SceneInstaller: scene installed successfully");
    }

    private void InstallCraftTables()
    {
        for (int i = 0; i < craftTables.Length; i++)
        {
            CraftTableBinding binding = craftTables[i];

            if (binding.Table == null)
            {
                Debug.LogError("SceneInstaller: Craft table is missing at index " + i);
                continue;
            }

            if (binding.Panel == null)
            {
                Debug.LogError("SceneInstaller: Craft panel is missing at index " + i);
                continue;
            }

            binding.Panel.Configure(rootCanvas);
            binding.Panel.Close();

            binding.Table.Configure(
                binding.TableId,
                binding.RoomId,
                binding.Panel,
                binding.LinkedDoor
            );
        }
    }
}

/// <summary>
/// Связка физического стола, его UI-панели, комнаты и двери.
/// </summary>
[Serializable]
public sealed class CraftTableBinding
{
    [Header("Ids")]
    public string TableId = "table_01";
    public string RoomId = "room_01";

    [Header("Scene Objects")]
    public CraftTableInteractable Table;
    public CraftingPanelUI Panel;
    public DoorInteractable LinkedDoor;
}