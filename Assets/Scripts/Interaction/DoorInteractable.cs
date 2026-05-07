using UnityEngine;

/// <summary>
/// Интерактивная дверь.
/// Открывается только при взаимодействии, если в инвентаре есть нужный ключевой элемент.
/// Ключевой элемент расходуется.
/// </summary>
public sealed class DoorInteractable : MonoBehaviour, IInteractable
{
    [Header("Unlock")]
    [SerializeField] private ElementDefinition requiredElement;

    [Header("Visual")]
    [SerializeField] private bool hideWhenUnlocked = true;
    [SerializeField] private Collider blockingCollider;
    [SerializeField] private Renderer visualRenderer;

    private bool unlocked;

    public string InteractionPrompt
    {
        get
        {
            if (unlocked)
                return "Door is open";

            if (requiredElement == null)
                return "Door is not configured";

            return "Use " + requiredElement.DisplayName + " to open";
        }
    }

    private void Awake()
    {
        if (blockingCollider == null)
            blockingCollider = GetComponent<Collider>();

        if (visualRenderer == null)
            visualRenderer = GetComponent<Renderer>();
    }

    public void Interact(PlayerInteractor interactor)
    {
        if (unlocked)
        {
            Debug.Log("Door is already open");
            GameAudio.Play(GameSoundId.CraftFail);
            return;
        }

        if (requiredElement == null)
        {
            Debug.LogError(gameObject.name + ": DoorInteractable has no required element");
            return;
        }

        if (Inventory.Instance == null)
        {
            Debug.LogError("DoorInteractable: Inventory instance not found");
            return;
        }

        bool consumed = Inventory.Instance.TryConsumeElement(requiredElement);

        if (!consumed)
        {
            Debug.Log("Door is locked. Required element: " + requiredElement.DisplayName);
            GameAudio.Play(GameSoundId.Locked);
            return;
        }

        Unlock();
    }

    private void Unlock()
    {
        unlocked = true;

        Debug.Log("Door opened: " + gameObject.name);
        GameAudio.Play(GameSoundId.DoorOpen);

        if (blockingCollider != null)
            blockingCollider.enabled = false;

        if (hideWhenUnlocked)
        {
            gameObject.SetActive(false);
            return;
        }

        if (visualRenderer != null)
            visualRenderer.enabled = false;
    }
}
