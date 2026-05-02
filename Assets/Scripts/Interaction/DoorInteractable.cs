using UnityEngine;

/// <summary>
/// Дверь больше не открывает стол.
/// Она только реагирует на создание нужного ключевого элемента.
/// </summary>
public sealed class DoorInteractable : MonoBehaviour, IInteractable
{
    [Header("Room")]
    [SerializeField] private string roomId = "room_01";

    [Header("Unlock")]
    [SerializeField] private InventoryItemType requiredKeyElementId = InventoryItemType.KeyCore;

    [Header("Visual")]
    [SerializeField] private bool hideWhenUnlocked = true;

    private bool unlocked;

    public string RoomId
    {
        get { return roomId; }
    }

    public InventoryItemType RequiredKeyElementId
    {
        get { return requiredKeyElementId; }
    }

    public bool IsUnlocked
    {
        get { return unlocked; }
    }

    public string InteractionPrompt
    {
        get { return unlocked ? "Door is open" : "Door is locked"; }
    }

    public void Configure(string roomId, InventoryItemType requiredKeyElementId)
    {
        this.roomId = roomId;
        this.requiredKeyElementId = requiredKeyElementId;
    }

    public void Interact(PlayerInteractor interactor)
    {
        if (unlocked)
        {
            Debug.Log("Door is already open");
            return;
        }

        Debug.Log("Door is locked. Create key element: " + requiredKeyElementId);
    }

    public void NotifyCraftedElement(InventoryItemType elementId)
    {
        if (unlocked)
            return;

        if (elementId != requiredKeyElementId)
            return;

        Unlock();
    }

    private void Unlock()
    {
        unlocked = true;

        Debug.Log("Door unlocked: " + gameObject.name);

        if (hideWhenUnlocked)
            gameObject.SetActive(false);
    }
}