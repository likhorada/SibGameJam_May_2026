using UnityEngine;

[DisallowMultipleComponent]
public sealed class FollowTransform : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private bool followPosition = true;
    [SerializeField] private bool followRotation = true;

    public void Initialize(Transform targetTransform)
    {
        target = targetTransform;
        Apply();
    }

    private void LateUpdate()
    {
        Apply();
    }

    private void Apply()
    {
        if (target == null)
            return;

        if (followPosition && followRotation)
        {
            transform.SetPositionAndRotation(target.position, target.rotation);
            return;
        }

        if (followPosition)
            transform.position = target.position;

        if (followRotation)
            transform.rotation = target.rotation;
    }
}
