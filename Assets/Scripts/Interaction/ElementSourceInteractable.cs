using UnityEngine;

/// <summary>
/// Бесконечный источник элемента.
/// При каждом взаимодействии кладёт один элемент в свободный слот инвентаря.
/// </summary>
public sealed class ElementSourceInteractable : MonoBehaviour, IInteractable
{
    [Header("Element")]
    [SerializeField] private InventoryItemType elementType;

    public string InteractionPrompt
    {
        get { return "Press E to collect " + Items.GetDisplayName(elementType); }
    }

    public void Configure(InventoryItemType elementType)
    {
        this.elementType = elementType;
    }

    public void Interact(PlayerInteractor interactor)
    {
        if (Inventory.Instance == null)
        {
            Debug.LogError("ElementSourceInteractable: Inventory instance not found");
            return;
        }

        Inventory.Instance.AddElement(elementType);
    }
}