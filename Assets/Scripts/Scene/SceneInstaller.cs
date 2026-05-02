using System;
using UnityEngine;

/// <summary>
/// Связывает объекты сцены.
/// Ничего не создаёт.
/// </summary>
public sealed class SceneInstaller : MonoBehaviour
{
    [Header("Systems")]
    [SerializeField] private Inventory inventory;
    [SerializeField] private CraftingSystem craftingSystem;
    [SerializeField] private CraftRecipeDatabase recipeDatabase;

    [Header("Core Elements")]
    [SerializeField] private ElementDefinition clayElement;

    [Header("UI")]
    [SerializeField] private Canvas rootCanvas;
    [SerializeField] private InventoryUI inventoryUI;

    [Header("Craft Tables")]
    [SerializeField] private CraftTableBinding[] craftTables;

    private void Start()
    {
        Install();
    }

    private void Install()
    {
        if (inventory == null)
            inventory = FindAnyObjectByType<Inventory>();

        if (craftingSystem == null)
            craftingSystem = FindAnyObjectByType<CraftingSystem>();

        if (inventory == null)
        {
            Debug.LogError("SceneInstaller: Inventory reference is missing");
            return;
        }

        if (craftingSystem == null)
        {
            Debug.LogError("SceneInstaller: CraftingSystem reference is missing");
            return;
        }

        if (recipeDatabase == null)
        {
            Debug.LogError("SceneInstaller: RecipeDatabase is missing");
            return;
        }

        if (clayElement == null)
        {
            Debug.LogError("SceneInstaller: Clay Element is missing");
            return;
        }

        if (rootCanvas == null)
        {
            Debug.LogError("SceneInstaller: Root Canvas is missing");
            return;
        }

        if (inventoryUI == null)
        {
            Debug.LogError("SceneInstaller: InventoryUI is missing");
            return;
        }

        inventory.Initialize();
        craftingSystem.Configure(recipeDatabase);
        inventoryUI.Configure(rootCanvas, clayElement);

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
                Debug.LogError("SceneInstaller: missing table at index " + i);
                continue;
            }

            if (binding.Panel == null)
            {
                Debug.LogError("SceneInstaller: missing panel at index " + i);
                continue;
            }

            binding.Panel.Configure(rootCanvas);
            binding.Panel.gameObject.SetActive(false);

            binding.Table.Configure(
                binding.TableId,
                binding.RoomId,
                binding.Panel
            );
        }
    }
}

/// <summary>
/// Связка физического стола и его UI-панели.
/// </summary>
[Serializable]
public sealed class CraftTableBinding
{
    public string TableId = "table_01";
    public string RoomId = "room_01";

    public CraftTableInteractable Table;
    public CraftingPanelUI Panel;
}