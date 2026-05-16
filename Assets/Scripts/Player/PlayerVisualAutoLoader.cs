using UnityEngine;

public sealed class PlayerVisualAutoLoader : MonoBehaviour
{
    [SerializeField] private string resourcePath = "Characters/Golem/GolemVisual";
    [SerializeField] private string fallbackResourcePath = "Characters/Golem/gogolem_re";
    [SerializeField] private string animationFallbackResourcePath = "Characters/Golem/gogolem";
    [SerializeField] private string visualInstanceName = "GolemVisual";
    [SerializeField] private Vector3 localPosition = Vector3.zero;
    [SerializeField] private Vector3 localEulerAngles = Vector3.zero;
    [SerializeField] private Vector3 localScale = Vector3.one;
    [SerializeField] private bool hideBuiltInRenderer = true;
    [SerializeField] private bool reuseExistingVisualChild = true;
    [SerializeField] private bool removeExtraVisualChildren = true;
    [SerializeField] private bool useRendererNameWhitelist = true;
    [SerializeField] private string[] visibleRendererNameContains = new[]
    {
        "Arm_L",
        "Arm_R",
        "Leg_L",
        "Leg_R",
        "Head",
        "Torso",
        "Weist"
    };
    [SerializeField] private bool logRendererSetup = true;
    [SerializeField] private bool playEmbeddedAnimationClips = true;
    [SerializeField] private PlayerEmbeddedClipAnimator embeddedClipAnimator = null;
    [SerializeField] private bool addProceduralAnimator = false;

    private GameObject visualInstance;
    private string loadedResourcePath;

    private void Awake()
    {
        GameObject visualPrefab = LoadVisualPrefab(out loadedResourcePath);

        if (visualPrefab == null)
            return;

        visualInstance = reuseExistingVisualChild ? FindExistingVisualChild(loadedResourcePath) : null;

        if (visualInstance == null)
        {
            visualInstance = Instantiate(visualPrefab, transform);
        }

        visualInstance.name = GetVisualInstanceName(visualPrefab);
        visualInstance.transform.localPosition = localPosition;
        visualInstance.transform.localRotation = Quaternion.Euler(localEulerAngles);
        visualInstance.transform.localScale = localScale;

        if (removeExtraVisualChildren)
            RemoveExtraVisualChildren(visualInstance);

        ApplyRendererFilter();

        if (hideBuiltInRenderer)
            HideBuiltInRenderer();

        if (playEmbeddedAnimationClips)
            ConfigureEmbeddedClipAnimator();

        if (addProceduralAnimator && visualInstance.GetComponent<PlayerProceduralAnimator>() == null)
            visualInstance.AddComponent<PlayerProceduralAnimator>();
    }

    private void ConfigureEmbeddedClipAnimator()
    {
        PlayerEmbeddedClipAnimator clipAnimator = embeddedClipAnimator;

        if (clipAnimator == null)
            clipAnimator = GetComponent<PlayerEmbeddedClipAnimator>();

        if (clipAnimator == null)
            clipAnimator = gameObject.AddComponent<PlayerEmbeddedClipAnimator>();

        clipAnimator.Configure(visualInstance.transform, loadedResourcePath, animationFallbackResourcePath);
    }

    private GameObject FindExistingVisualChild(string resourcePath)
    {
        string resourceName = GetResourceName(resourcePath);
        string fallbackName = GetResourceName(fallbackResourcePath);
        string animationFallbackName = GetResourceName(animationFallbackResourcePath);

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);

            if (IsVisualChildName(child.name, resourceName, fallbackName, animationFallbackName))
                return child.gameObject;
        }

        return null;
    }

    private void RemoveExtraVisualChildren(GameObject keep)
    {
        string resourceName = GetResourceName(loadedResourcePath);
        string fallbackName = GetResourceName(fallbackResourcePath);
        string animationFallbackName = GetResourceName(animationFallbackResourcePath);

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);

            if (child.gameObject == keep)
                continue;

            if (IsVisualChildName(child.name, resourceName, fallbackName, animationFallbackName))
                Destroy(child.gameObject);
        }
    }

    private void ApplyRendererFilter()
    {
        Renderer[] renderers = visualInstance.GetComponentsInChildren<Renderer>(true);
        int disabledCount = 0;

        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];

            if (renderer == null)
                continue;

            bool shouldBeVisible = !useRendererNameWhitelist || IsWhitelistedRenderer(renderer.transform);
            renderer.enabled = shouldBeVisible;

            if (!shouldBeVisible)
                disabledCount++;
        }

        if (logRendererSetup)
            Debug.Log(gameObject.name + ": visual renderers active " + (renderers.Length - disabledCount) + "/" + renderers.Length + BuildRendererLog(renderers));
    }

    private bool IsWhitelistedRenderer(Transform rendererTransform)
    {
        if (visibleRendererNameContains == null || visibleRendererNameContains.Length == 0)
            return true;

        Transform current = rendererTransform;

        while (current != null && current != visualInstance.transform.parent)
        {
            for (int i = 0; i < visibleRendererNameContains.Length; i++)
            {
                string namePart = visibleRendererNameContains[i];

                if (!string.IsNullOrWhiteSpace(namePart)
                    && current.name.IndexOf(namePart, System.StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return true;
                }
            }

            current = current.parent;
        }

        return false;
    }

    private string BuildRendererLog(Renderer[] renderers)
    {
        if (renderers == null || renderers.Length == 0)
            return "";

        string result = "";

        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer renderer = renderers[i];

            if (renderer == null)
                continue;

            result += "\n- " + GetPath(renderer.transform) + " enabled=" + renderer.enabled;
        }

        return result;
    }

    private string GetPath(Transform target)
    {
        if (target == null)
            return "";

        string path = target.name;
        Transform current = target.parent;

        while (current != null && current != transform)
        {
            path = current.name + "/" + path;
            current = current.parent;
        }

        return path;
    }

    private bool IsVisualChildName(string childName, string resourceName, string fallbackName, string animationFallbackName)
    {
        if (string.IsNullOrWhiteSpace(childName))
            return false;

        return NamesMatch(childName, visualInstanceName)
            || NamesMatch(childName, resourceName)
            || NamesMatch(childName, fallbackName)
            || NamesMatch(childName, animationFallbackName);
    }

    private string GetVisualInstanceName(GameObject visualPrefab)
    {
        if (!string.IsNullOrWhiteSpace(visualInstanceName))
            return visualInstanceName;

        return visualPrefab == null ? "GolemVisual" : visualPrefab.name;
    }

    private GameObject LoadVisualPrefab(out string loadedPath)
    {
        loadedPath = null;

        GameObject visualPrefab = LoadResource(resourcePath);

        if (visualPrefab != null)
        {
            loadedPath = resourcePath;
            return visualPrefab;
        }

        visualPrefab = LoadResource(fallbackResourcePath);

        if (visualPrefab != null)
            loadedPath = fallbackResourcePath;

        return visualPrefab;
    }

    private static GameObject LoadResource(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return null;

        return Resources.Load<GameObject>(path);
    }

    private static string GetResourceName(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return null;

        int slashIndex = path.LastIndexOf('/');
        return slashIndex >= 0 ? path.Substring(slashIndex + 1) : path;
    }

    private static bool NamesMatch(string a, string b)
    {
        return !string.IsNullOrWhiteSpace(a)
            && !string.IsNullOrWhiteSpace(b)
            && string.Equals(a, b, System.StringComparison.OrdinalIgnoreCase);
    }

    private void HideBuiltInRenderer()
    {
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
            meshRenderer.enabled = false;
    }
}
