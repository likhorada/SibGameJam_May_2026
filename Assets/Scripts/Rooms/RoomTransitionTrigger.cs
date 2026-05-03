using UnityEngine;

/// <summary>
/// Триггер перехода между комнатами.
/// Переносит игрока в targetSpawnPoint и двигает камеру к targetCameraPoint.
/// Игрок определяется по компоненту PlayerMovement, а не по имени объекта.
/// </summary>
[RequireComponent(typeof(Collider))]
public sealed class RoomTransitionTrigger : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform targetSpawnPoint;
    [SerializeField] private Transform targetCameraPoint;

    [Header("References")]
    [SerializeField] private RoomCameraController cameraController;

    [Header("Debug")]
    [SerializeField] private bool logTransitions = true;

    private bool isTransitioning;

    private void Awake()
    {
        Collider triggerCollider = GetComponent<Collider>();
        triggerCollider.isTrigger = true;

        if (cameraController == null)
            cameraController = FindAnyObjectByType<RoomCameraController>();
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerMovement playerMovement = other.GetComponent<PlayerMovement>();

        if (playerMovement == null)
            playerMovement = other.GetComponentInParent<PlayerMovement>();

        if (playerMovement == null)
            return;

        if (isTransitioning)
            return;

        if (targetSpawnPoint == null)
        {
            Debug.LogError(gameObject.name + ": Target Spawn Point is not assigned");
            return;
        }

        if (targetCameraPoint == null)
        {
            Debug.LogError(gameObject.name + ": Target Camera Point is not assigned");
            return;
        }

        if (cameraController == null)
        {
            Debug.LogError(gameObject.name + ": Camera Controller is not assigned");
            return;
        }

        StartTransition(playerMovement.gameObject);
    }

    private void StartTransition(GameObject player)
    {
        isTransitioning = true;

        if (logTransitions)
            Debug.Log(gameObject.name + ": transition to " + targetSpawnPoint.name);

        Rigidbody playerRigidbody = player.GetComponent<Rigidbody>();

        if (playerRigidbody != null)
        {
#if UNITY_6000_0_OR_NEWER
            playerRigidbody.linearVelocity = Vector3.zero;
#else
            playerRigidbody.velocity = Vector3.zero;
#endif
            playerRigidbody.angularVelocity = Vector3.zero;

            playerRigidbody.position = targetSpawnPoint.position;
            playerRigidbody.rotation = targetSpawnPoint.rotation;
        }
        else
        {
            player.transform.position = targetSpawnPoint.position;
            player.transform.rotation = targetSpawnPoint.rotation;
        }

        cameraController.MoveToCameraPoint(targetCameraPoint);

        Invoke(nameof(UnlockTransition), 0.35f);
    }

    private void UnlockTransition()
    {
        isTransitioning = false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(Vector3.zero, Vector3.one);

        Gizmos.matrix = Matrix4x4.identity;

        if (targetSpawnPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, targetSpawnPoint.position);
        }

        if (targetCameraPoint != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, targetCameraPoint.position);
        }
    }
}