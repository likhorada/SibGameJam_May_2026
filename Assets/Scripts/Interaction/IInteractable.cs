/// <summary>
/// Общий интерфейс интерактивных объектов.
/// </summary>
public interface IInteractable
{
    string InteractionPrompt { get; }

    void Interact(PlayerInteractor interactor);
}