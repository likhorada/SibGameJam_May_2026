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

    [Header("Audio")]
    [SerializeField] private GameAudioProfile gameAudioProfile;
    [SerializeField] private AudioSource gameAudioSource;
    [SerializeField, Range(0f, 1f)] private float gameAudioVolume = 1f;

    [Header("UI")]
    [SerializeField] private Canvas rootCanvas;
    [SerializeField] private InventoryUI inventoryUI;
    [Tooltip("Optional scene instance. If empty, SceneInstaller uses Pause Menu Prefab or creates a runtime fallback.")]
    [SerializeField] private PauseMenuController pauseMenuController;
    [Tooltip("Optional prefab used when no PauseMenuController exists under the root Canvas.")]
    [SerializeField] private PauseMenuController pauseMenuPrefab;
    [Tooltip("Optional scene instance. If empty, SceneInstaller uses Interaction Hint Window Prefab or lets InteractionHintWindow create a runtime fallback.")]
    [SerializeField] private InteractionHintWindow interactionHintWindow;
    [Tooltip("Optional prefab used when no InteractionHintWindow exists in the scene.")]
    [SerializeField] private InteractionHintWindow interactionHintWindowPrefab;

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
        InstallAudio();
        inventoryUI.Configure(rootCanvas, clayElement);
        InstallPauseMenu();
        InstallInteractionHintWindow();

        InstallCraftTables();

        Debug.Log("SceneInstaller: scene installed successfully");
    }

    private void InstallAudio()
    {
        if (gameAudioProfile == null && gameAudioSource == null)
            return;

        GameAudio.Configure(gameAudioSource, gameAudioProfile, gameAudioVolume);
    }

    private void InstallPauseMenu()
    {
        if (pauseMenuController != null && !pauseMenuController.gameObject.scene.IsValid())
        {
            PauseMenuController prefabReference = pauseMenuController;
            pauseMenuController = Instantiate(prefabReference, rootCanvas.transform);
            pauseMenuController.name = prefabReference.name;
        }

        if (pauseMenuController == null)
            pauseMenuController = rootCanvas.GetComponentInChildren<PauseMenuController>(true);

        if (pauseMenuController == null && pauseMenuPrefab != null)
        {
            pauseMenuController = Instantiate(pauseMenuPrefab, rootCanvas.transform);
            pauseMenuController.name = pauseMenuPrefab.name;
        }

        if (pauseMenuController == null)
        {
            GameObject pauseMenuObject = new GameObject("PauseMenuController");
            pauseMenuObject.transform.SetParent(rootCanvas.transform, false);
            pauseMenuController = pauseMenuObject.AddComponent<PauseMenuController>();
        }

        pauseMenuController.Configure(rootCanvas);
    }

    private void InstallInteractionHintWindow()
    {
        if (interactionHintWindow != null && !interactionHintWindow.gameObject.scene.IsValid())
        {
            InteractionHintWindow prefabReference = interactionHintWindow;
            interactionHintWindow = Instantiate(prefabReference);
            interactionHintWindow.name = prefabReference.name;
        }

        if (interactionHintWindow == null)
            interactionHintWindow = FindAnyObjectByType<InteractionHintWindow>();

        if (interactionHintWindow == null && interactionHintWindowPrefab != null)
        {
            interactionHintWindow = Instantiate(interactionHintWindowPrefab);
            interactionHintWindow.name = interactionHintWindowPrefab.name;
        }
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
