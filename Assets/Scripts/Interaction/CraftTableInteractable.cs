using UnityEngine;

/// <summary>
/// Отдельный интерактивный стол крафта.
/// Он не зависит от двери и остаётся доступен всегда, пока игрок может к нему подойти.
/// </summary>
public sealed class CraftTableInteractable : MonoBehaviour, IInteractable
{
    [Header("Table")]
    [SerializeField] private string tableId = "table_01";
    [SerializeField] private string roomId = "room_01";

    [Header("UI")]
    [SerializeField] private CraftingPanelUI craftingPanel;

    [Header("Linked Door")]
    [SerializeField] private DoorInteractable linkedDoor;

    public string InteractionPrompt
    {
        get { return "Press E to use craft table"; }
    }

    public string TableId
    {
        get { return tableId; }
    }

    public string RoomId
    {
        get { return roomId; }
    }

    public void Configure(
        string newTableId,
        string newRoomId,
        CraftingPanelUI newCraftingPanel,
        DoorInteractable newLinkedDoor)
    {
        tableId = newTableId;
        roomId = newRoomId;
        craftingPanel = newCraftingPanel;
        linkedDoor = newLinkedDoor;
    }

    public void SetCraftingPanel(CraftingPanelUI newCraftingPanel)
    {
        craftingPanel = newCraftingPanel;
    }

    public void SetLinkedDoor(DoorInteractable newLinkedDoor)
    {
        linkedDoor = newLinkedDoor;
    }

    public void Interact(PlayerInteractor interactor)
    {
        if (craftingPanel == null)
        {
            Debug.LogError("CraftTableInteractable: CraftingPanelUI is not assigned");
            return;
        }

        craftingPanel.Open(tableId, roomId, linkedDoor);
    }
}