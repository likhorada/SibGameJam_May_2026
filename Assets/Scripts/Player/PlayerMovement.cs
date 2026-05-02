using UnityEngine;

/// <summary>
/// Движение игрока через Rigidbody.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public sealed class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;

    private Rigidbody rb;
    private Vector3 inputDirection;
    private Vector3 lastLookDirection = Vector3.forward;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        inputDirection = new Vector3(horizontal, 0f, vertical).normalized;

        if (inputDirection.sqrMagnitude > 0.001f)
            lastLookDirection = inputDirection;
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