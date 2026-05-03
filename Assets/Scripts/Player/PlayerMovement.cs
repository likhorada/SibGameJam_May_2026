using UnityEngine;

/// <summary>
/// Двигает игрока через Rigidbody без телепортации.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public sealed class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Transform cameraTransform;

    private Rigidbody rb;
    private Vector3 inputDirection;
    private Vector3 lastLookDirection = Vector3.forward;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (CraftingPanelUI.HasOpenPanel)
        {
            inputDirection = Vector3.zero;
            return;
        }

        Camera activeCamera = GetActiveCamera();
        if (activeCamera != null)
            cameraTransform = activeCamera.transform;

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 rawInput = new Vector3(horizontal, 0f, vertical);
        if (rawInput.sqrMagnitude > 1f)
            rawInput.Normalize();

        if (cameraTransform != null)
        {
            Vector3 cameraForward = cameraTransform.forward;
            Vector3 cameraRight = cameraTransform.right;
            cameraForward.y = 0f;
            cameraRight.y = 0f;
            cameraForward.Normalize();
            cameraRight.Normalize();

            inputDirection = (cameraForward * rawInput.z + cameraRight * rawInput.x).normalized;
        }
        else
        {
            inputDirection = rawInput.normalized;
        }

        if (inputDirection.sqrMagnitude > 0.001f)
            lastLookDirection = inputDirection;
    }

    private static Camera GetActiveCamera()
    {
        Camera[] cameras = Camera.allCameras;
        Camera bestCamera = null;
        float highestDepth = float.MinValue;

        for (int i = 0; i < cameras.Length; i++)
        {
            Camera camera = cameras[i];
            if (!camera.isActiveAndEnabled)
                continue;

            if (camera.depth > highestDepth)
            {
                highestDepth = camera.depth;
                bestCamera = camera;
            }
        }

        return bestCamera;
    }

    private void FixedUpdate()
    {
        Vector3 nextPosition = rb.position + inputDirection * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(nextPosition);

        if (lastLookDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lastLookDirection, Vector3.up);
            rb.MoveRotation(targetRotation);
        }
    }
}
