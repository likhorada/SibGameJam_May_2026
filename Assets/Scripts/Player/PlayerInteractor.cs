using UnityEngine;

/// <summary>
/// Взаимодействие по E. Сначала Raycast вперёд, затем fallback через поиск рядом.
/// </summary>
public sealed class PlayerInteractor : MonoBehaviour
{
    [SerializeField] private float interactDistance = 2.4f;
    [SerializeField] private float fallbackRadius = 1.6f;
    [SerializeField] private Vector3 rayOffset = new Vector3(0f, 0.45f, 0f);

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
            TryInteract();
    }

    private void TryInteract()
    {
        IInteractable interactable = FindByRaycast();

        if (interactable == null)
            interactable = FindNearestByOverlap();

        if (interactable == null)
        {
            Debug.Log("No interactable object nearby");
            return;
        }

        interactable.Interact(this);
    }

    private IInteractable FindByRaycast()
    {
        Ray ray = new Ray(transform.position + rayOffset, transform.forward);

        if (!Physics.Raycast(ray, out RaycastHit hit, interactDistance))
            return null;

        return hit.collider.GetComponent<IInteractable>();
    }

    private IInteractable FindNearestByOverlap()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, fallbackRadius);

        IInteractable nearestInteractable = null;
        float nearestDistance = float.MaxValue;

        for (int i = 0; i < hits.Length; i++)
        {
            IInteractable interactable = hits[i].GetComponent<IInteractable>();

            if (interactable == null)
                continue;

            float distance = Vector3.Distance(transform.position, hits[i].transform.position);

            if (distance >= nearestDistance)
                continue;

            nearestDistance = distance;
            nearestInteractable = interactable;
        }

        return nearestInteractable;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position + rayOffset, transform.forward * interactDistance);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, fallbackRadius);
    }
}