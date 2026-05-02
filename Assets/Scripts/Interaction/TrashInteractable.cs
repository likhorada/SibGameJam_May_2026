using UnityEngine;

/// <summary>
/// Мусорка.
/// При взаимодействии очищает все 4 обычных слота инвентаря.
/// Вечный слот глины не очищается.
/// </summary>
public sealed class TrashInteractable : MonoBehaviour, IInteractable
{
    public string InteractionPrompt
    {
        get { return "Press E to clear inventory"; }
    }

    public void Interact(PlayerInteractor interactor)
    {
        if (Inventory.Instance == null)
        {
            Debug.LogError("TrashInteractable: Inventory instance not found");
            return;
        }

        Inventory.Instance.ClearAllNormalSlots();
        Debug.Log("TrashInteractable: inventory cleared");
    }
}