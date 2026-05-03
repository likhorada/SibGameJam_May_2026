using UnityEngine;

/// <summary>
/// Некрафтовый стол-пьедестал.
/// При взаимодействии принимает один из нужных предметов из инвентаря,
/// расходует его и показывает 3D-визуал на столе.
/// </summary>
public sealed class OfferingTableInteractable : MonoBehaviour, IInteractable
{
    [Header("Required Items")]
    [SerializeField] private ElementDefinition[] requiredItems = new ElementDefinition[3];

    [Header("Spawn Points")]
    [SerializeField] private Transform[] itemAnchors = new Transform[3];

    [Header("Fallback Visual")]
    [SerializeField] private Vector3 fallbackCubeScale = new Vector3(0.35f, 0.35f, 0.35f);
    [SerializeField] private float fallbackVerticalOffset = 0.35f;

    private bool[] placed;
    private GameObject[] spawnedVisuals;

    public string InteractionPrompt
    {
        get { return "Press E to place required item"; }
    }

    private void Awake()
    {
        EnsureRuntimeState();
    }

    private void OnValidate()
    {
        if (requiredItems == null)
            requiredItems = new ElementDefinition[0];

        if (itemAnchors == null)
        {
            itemAnchors = new Transform[requiredItems.Length];
            return;
        }

        if (itemAnchors.Length == requiredItems.Length)
            return;

        Transform[] resizedAnchors = new Transform[requiredItems.Length];
        int copyCount = Mathf.Min(itemAnchors.Length, resizedAnchors.Length);

        for (int i = 0; i < copyCount; i++)
            resizedAnchors[i] = itemAnchors[i];

        itemAnchors = resizedAnchors;
    }

    public void Interact(PlayerInteractor interactor)
    {
        EnsureRuntimeState();

        if (Inventory.Instance == null)
        {
            Debug.LogError("OfferingTableInteractable: Inventory instance not found");
            return;
        }

        if (IsCompleted())
        {
            Debug.Log(gameObject.name + ": all required items are already placed");
            return;
        }

        int itemIndex = FindFirstAvailableRequiredItemInInventory();

        if (itemIndex < 0)
        {
            Debug.Log(gameObject.name + ": no required item in inventory");
            return;
        }

        ElementDefinition element = requiredItems[itemIndex];

        bool consumed = Inventory.Instance.TryConsumeElement(element);

        if (!consumed)
        {
            Debug.Log(gameObject.name + ": failed to consume " + element.DisplayName);
            return;
        }

        placed[itemIndex] = true;
        SpawnItemVisual(itemIndex, element);

        Debug.Log(gameObject.name + ": placed " + element.DisplayName);

        if (IsCompleted())
            OnCompleted();
    }

    private int FindFirstAvailableRequiredItemInInventory()
    {
        for (int i = 0; i < requiredItems.Length; i++)
        {
            if (placed[i])
                continue;

            ElementDefinition required = requiredItems[i];

            if (required == null)
                continue;

            if (Inventory.Instance.HasElement(required))
                return i;
        }

        return -1;
    }

    private void SpawnItemVisual(int index, ElementDefinition element)
    {
        EnsureRuntimeState();

        if (spawnedVisuals[index] != null)
            Destroy(spawnedVisuals[index]);

        Transform anchor = GetAnchor(index);

        GameObject visual;

        if (element.WorldPrefab != null)
        {
            visual = Instantiate(element.WorldPrefab, anchor.position, anchor.rotation, anchor);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;
            visual.transform.localScale = element.WorldScale;
        }
        else
        {
            visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.name = "Visual_" + element.Id;
            visual.transform.SetParent(anchor, false);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;
            visual.transform.localScale = fallbackCubeScale;

            Renderer renderer = visual.GetComponent<Renderer>();

            if (renderer != null)
                renderer.material.color = element.FallbackColor;
        }

        spawnedVisuals[index] = visual;
    }

    private Transform GetAnchor(int index)
    {
        if (itemAnchors != null
            && index >= 0
            && index < itemAnchors.Length
            && itemAnchors[index] != null)
        {
            return itemAnchors[index];
        }

        GameObject fallbackAnchor = new GameObject("AutoAnchor_" + index);
        fallbackAnchor.transform.SetParent(transform, false);

        float xOffset = (index - 1) * 0.55f;

        fallbackAnchor.transform.localPosition =
            new Vector3(xOffset, fallbackVerticalOffset, 0f);

        fallbackAnchor.transform.localRotation = Quaternion.identity;

        return fallbackAnchor.transform;
    }

    private bool IsCompleted()
    {
        if (requiredItems == null || requiredItems.Length == 0)
            return false;

        for (int i = 0; i < requiredItems.Length; i++)
        {
            if (requiredItems[i] == null)
                return false;

            if (!placed[i])
                return false;
        }

        return true;
    }

    private void OnCompleted()
    {
        Debug.Log(gameObject.name + ": offering table completed");

        // Здесь позже можно:
        // - открыть проход;
        // - активировать кат-сцену;
        // - запустить событие комнаты;
        // - включить следующий механизм.
    }

    private void EnsureRuntimeState()
    {
        int count = requiredItems == null ? 0 : requiredItems.Length;

        if (placed == null || placed.Length != count)
            placed = ResizeStateArray(placed, count);

        if (spawnedVisuals == null || spawnedVisuals.Length != count)
            spawnedVisuals = ResizeVisualArray(spawnedVisuals, count);
    }

    private static bool[] ResizeStateArray(bool[] source, int size)
    {
        bool[] result = new bool[size];

        if (source == null)
            return result;

        int copyCount = Mathf.Min(source.Length, result.Length);

        for (int i = 0; i < copyCount; i++)
            result[i] = source[i];

        return result;
    }

    private static GameObject[] ResizeVisualArray(GameObject[] source, int size)
    {
        GameObject[] result = new GameObject[size];

        if (source == null)
            return result;

        int copyCount = Mathf.Min(source.Length, result.Length);

        for (int i = 0; i < copyCount; i++)
            result[i] = source[i];

        return result;
    }
}
