using UnityEngine;

/// <summary>
/// Управляет камерой комнат.
/// Камера плавно переезжает к указанной CameraPoint.
/// </summary>
public sealed class RoomCameraController : MonoBehaviour
{
    [System.Serializable]
    private sealed class CameraTransitionRule
    {
        [Tooltip("CameraPoint, from which the transition starts.")]
        public Transform fromCameraPoint;

        [Tooltip("CameraPoint, to which the transition goes.")]
        public Transform toCameraPoint;

        [Tooltip("Transition mode for this exact pair.")]
        public CameraTransitionMode transitionMode = CameraTransitionMode.Snap;
    }

    public enum CameraTransitionMode
    {
        Smooth,
        Snap
    }

    [Header("Camera")]
    [SerializeField] private Camera targetCamera;

    [Header("Transition")]
    [Tooltip("Used when no rule matches the current CameraPoint and target CameraPoint.")]
    [SerializeField] private CameraTransitionMode transitionMode = CameraTransitionMode.Snap;

    [Tooltip("Optional exact from/to overrides for camera transitions.")]
    [SerializeField] private CameraTransitionRule[] transitionRules = new CameraTransitionRule[0];

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float rotateSpeed = 8f;

    [Header("Default View")]
    [SerializeField] private Transform initialCameraPoint;
    [SerializeField] private bool snapToInitialPointOnStart = true;

    [Header("Debug")]
    [SerializeField] private bool logCameraMoves = true;

    private Transform currentCameraPoint;
    private CameraTransitionMode currentTransitionMode;

    private void Awake()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        currentTransitionMode = transitionMode;
    }

    private void Start()
    {
        currentCameraPoint = initialCameraPoint;

        if (snapToInitialPointOnStart && currentCameraPoint != null)
            SnapToCameraPoint(currentCameraPoint);
    }

    private void LateUpdate()
    {
        if (targetCamera == null || currentCameraPoint == null)
            return;

        Transform cameraTransform = targetCamera.transform;

        if (currentTransitionMode == CameraTransitionMode.Snap)
        {
            SnapCameraTransform(cameraTransform, currentCameraPoint);
            return;
        }

        cameraTransform.position = Vector3.Lerp(
            cameraTransform.position,
            currentCameraPoint.position,
            moveSpeed * Time.deltaTime
        );

        cameraTransform.rotation = Quaternion.Slerp(
            cameraTransform.rotation,
            currentCameraPoint.rotation,
            rotateSpeed * Time.deltaTime
        );
    }

    public void MoveToCameraPoint(Transform cameraPoint)
    {
        if (cameraPoint == null)
        {
            Debug.LogError("RoomCameraController: cameraPoint is null");
            return;
        }

        CameraTransitionMode nextTransitionMode = GetTransitionMode(currentCameraPoint, cameraPoint);

        currentCameraPoint = cameraPoint;
        currentTransitionMode = nextTransitionMode;

        if (currentTransitionMode == CameraTransitionMode.Snap)
        {
            SnapToCameraPoint(cameraPoint);
            return;
        }

        if (logCameraMoves)
            Debug.Log("RoomCameraController: moving to " + cameraPoint.name + " using " + currentTransitionMode);
    }

    public void SnapToCameraPoint(Transform cameraPoint)
    {
        if (targetCamera == null)
        {
            Debug.LogError("RoomCameraController: Target Camera is missing");
            return;
        }

        if (cameraPoint == null)
        {
            Debug.LogError("RoomCameraController: Camera Point is missing");
            return;
        }

        SnapCameraTransform(targetCamera.transform, cameraPoint);

        currentCameraPoint = cameraPoint;
        currentTransitionMode = CameraTransitionMode.Snap;

        if (logCameraMoves)
            Debug.Log("RoomCameraController: snapped to " + cameraPoint.name);
    }

    private CameraTransitionMode GetTransitionMode(Transform fromCameraPoint, Transform toCameraPoint)
    {
        if (fromCameraPoint == null || toCameraPoint == null)
            return transitionMode;

        for (int i = 0; i < transitionRules.Length; i++)
        {
            CameraTransitionRule rule = transitionRules[i];

            if (rule == null)
                continue;

            if (rule.fromCameraPoint == fromCameraPoint && rule.toCameraPoint == toCameraPoint)
                return rule.transitionMode;
        }

        return transitionMode;
    }

    private static void SnapCameraTransform(Transform cameraTransform, Transform cameraPoint)
    {
        cameraTransform.position = cameraPoint.position;
        cameraTransform.rotation = cameraPoint.rotation;
    }
}
