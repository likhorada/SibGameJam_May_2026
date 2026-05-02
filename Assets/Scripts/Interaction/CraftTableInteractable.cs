using UnityEngine;

/// <summary>
/// Интерактивный стол крафта.
/// Может быть активным сразу или требовать элемент для активации.
/// Пример: котёл во второй комнате требует факел.
/// </summary>
public sealed class CraftTableInteractable : MonoBehaviour, IInteractable
{
    [Header("Table")]
    [SerializeField] private string tableId = "table_01";
    [SerializeField] private string roomId = "room_01";
    [SerializeField] private CraftingPanelUI craftingPanel;

    [Header("Activation")]
    [SerializeField] private bool startsActive = true;
    [SerializeField] private ElementDefinition requiredActivationElement;
    [SerializeField] private bool consumeActivationElement = true;

    [Header("Visual")]
    [SerializeField] private Renderer visualRenderer;
    [SerializeField] private Color activeColor = Color.white;
    [SerializeField] private Color inactiveColor = Color.black;

    private bool active;

    public string TableId
    {
        get { return tableId; }
    }

    public string RoomId
    {
        get { return roomId; }
    }

    public bool IsActive
    {
        get { return active; }
    }

    public string InteractionPrompt
    {
        get
        {
            if (active)
                return "Use craft table";

            if (requiredActivationElement == null)
                return "Craft table is inactive";

            return "Activate with " + requiredActivationElement.DisplayName;
        }
    }

    private void Awake()
    {
        if (visualRenderer == null)
            visualRenderer = GetComponent<Renderer>();

        active = startsActive;
        RefreshVisual();
    }

    public void Configure(
        string newTableId,
        string newRoomId,
        CraftingPanelUI newCraftingPanel)
    {
        tableId = newTableId;
        roomId = newRoomId;
        craftingPanel = newCraftingPanel;
    }

    public void Interact(PlayerInteractor interactor)
    {
        if (!active)
        {
            TryActivate();
            return;
        }

        if (craftingPanel == null)
        {
            Debug.LogError(gameObject.name + ": CraftTableInteractable has no CraftingPanelUI");
            return;
        }

        craftingPanel.Open(tableId, roomId);
    }

    private void TryActivate()
    {
        if (requiredActivationElement == null)
        {
            Debug.Log("Craft table is inactive and has no activation element configured");
            return;
        }

        if (Inventory.Instance == null)
        {
            Debug.LogError("CraftTableInteractable: Inventory instance not found");
            return;
        }

        bool hasElement = Inventory.Instance.HasElement(requiredActivationElement);

        if (!hasElement)
        {
            Debug.Log("Need " + requiredActivationElement.DisplayName + " to activate " + gameObject.name);
            return;
        }

        if (consumeActivationElement)
            Inventory.Instance.TryConsumeElement(requiredActivationElement);

        active = true;
        RefreshVisual();

        Debug.Log("Craft table activated: " + gameObject.name);
    }

    private void RefreshVisual()
    {
        if (visualRenderer == null)
            return;

        visualRenderer.material.color = active ? activeColor : inactiveColor;
    }
}