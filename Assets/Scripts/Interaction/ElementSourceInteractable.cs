using UnityEngine;

/// <summary>
/// Бесконечный источник элемента.
/// При взаимодействии добавляет элемент в свободный слот инвентаря.
/// </summary>
public sealed class ElementSourceInteractable : MonoBehaviour, IInteractable
{
    [Header("Element")]
    [SerializeField] private ElementDefinition element;

    public string InteractionPrompt
    {
        get
        {
            if (element == null)
                return "Element source is not configured";

            return "Press E to collect " + element.DisplayName;
        }
    }

    public void Interact(PlayerInteractor interactor)
    {
        if (element == null)
        {
            Debug.LogError(gameObject.name + ": ElementSourceInteractable has no ElementDefinition");
            return;
        }

        if (Inventory.Instance == null)
        {
            Debug.LogError("ElementSourceInteractable: Inventory instance not found");
            return;
        }

        Inventory.Instance.AddElement(element);
    }
}