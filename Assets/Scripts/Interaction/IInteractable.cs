/// <summary>
/// Общий контракт для всех объектов, с которыми игрок может взаимодействовать.
/// </summary>
public interface IInteractable
{
    string InteractionPrompt { get; }

    void Interact(PlayerInteractor interactor);
}