using UnityEngine;

public sealed class PlayerVisualAutoLoader : MonoBehaviour
{
    [SerializeField] private string resourcePath = "Characters/Golem/GolemVisual";
    [SerializeField] private Vector3 localPosition = Vector3.zero;
    [SerializeField] private Vector3 localEulerAngles = Vector3.zero;
    [SerializeField] private Vector3 localScale = Vector3.one;
    [SerializeField] private bool hideBuiltInRenderer = true;

    private GameObject visualInstance;

    private void Awake()
    {
        GameObject visualPrefab = Resources.Load<GameObject>(resourcePath);
        if (visualPrefab == null)
            return;

        visualInstance = Instantiate(visualPrefab, transform);
        visualInstance.name = visualPrefab.name;
        visualInstance.transform.localPosition = localPosition;
        visualInstance.transform.localRotation = Quaternion.Euler(localEulerAngles);
        visualInstance.transform.localScale = localScale;

        if (hideBuiltInRenderer)
            HideBuiltInRenderer();
    }

    private void HideBuiltInRenderer()
    {
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer != null)
            meshRenderer.enabled = false;
    }
}
