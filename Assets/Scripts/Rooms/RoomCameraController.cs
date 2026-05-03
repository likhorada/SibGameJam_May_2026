using UnityEngine;

/// <summary>
/// Управляет камерой комнат.
/// Камера плавно переезжает к указанной CameraPoint.
/// </summary>
public sealed class RoomCameraController : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Camera targetCamera;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 6f;
    [SerializeField] private float rotateSpeed = 8f;

    [Header("Default View")]
    [SerializeField] private Transform initialCameraPoint;
    [SerializeField] private bool snapToInitialPointOnStart = true;

    [Header("Debug")]
    [SerializeField] private bool logCameraMoves = true;

    private Transform currentCameraPoint;

    private void Awake()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;
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

        currentCameraPoint = cameraPoint;

        if (logCameraMoves)
            Debug.Log("RoomCameraController: moving to " + cameraPoint.name);
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

        targetCamera.transform.position = cameraPoint.position;
        targetCamera.transform.rotation = cameraPoint.rotation;

        currentCameraPoint = cameraPoint;

        if (logCameraMoves)
            Debug.Log("RoomCameraController: snapped to " + cameraPoint.name);
    }
}