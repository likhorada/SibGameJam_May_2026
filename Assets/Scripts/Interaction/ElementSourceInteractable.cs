using UnityEngine;

/// <summary>
/// Бесконечный источник элемента.
/// При каждом взаимодействии кладёт один элемент в свободный слот инвентаря.
/// </summary>
public sealed class ElementSourceInteractable : MonoBehaviour, IInteractable
{
    [Header("Element")]
    [SerializeField] private string elementId;
    [SerializeField] private string elementName;

    public string InteractionPrompt
    {
        get { return "Press E to collect " + elementName; }
    }

    public void Configure(string newElementId, string newElementName)
    {
        elementId = newElementId;
        elementName = newElementName;
    }

    public void Interact(PlayerInteractor interactor)
    {
        if (Inventory.Instance == null)
        {
            Debug.LogError("ElementSourceInteractable: Inventory instance not found");
            return;
        }

        Inventory.Instance.AddElement(elementId, elementName);
    }
}