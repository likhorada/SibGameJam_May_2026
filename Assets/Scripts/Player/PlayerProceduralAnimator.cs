using UnityEngine;

public sealed class PlayerProceduralAnimator : MonoBehaviour
{
    [SerializeField] private Transform visualRoot;
    [SerializeField] private float walkThreshold = 0.05f;
    [SerializeField] private float blendSpeed = 9f;
    [SerializeField] private float idleBobAmplitude = 0.025f;
    [SerializeField] private float idleBobFrequency = 1.4f;
    [SerializeField] private float walkBobAmplitude = 0.08f;
    [SerializeField] private float walkBobFrequency = 7f;
    [SerializeField] private float walkPitchDegrees = 3f;
    [SerializeField] private float walkRollDegrees = 2.5f;

    private Rigidbody parentRigidbody;
    private Vector3 baseLocalPosition;
    private Quaternion baseLocalRotation;
    private float walkBlend;
    private float phase;

    private void Awake()
    {
        if (visualRoot == null)
            visualRoot = transform;

        parentRigidbody = GetComponentInParent<Rigidbody>();
        baseLocalPosition = visualRoot.localPosition;
        baseLocalRotation = visualRoot.localRotation;
    }

    private void LateUpdate()
    {
        if (visualRoot == null)
            return;

        float planarSpeed = GetPlanarSpeed();
        float targetBlend = planarSpeed > walkThreshold ? 1f : 0f;
        walkBlend = Mathf.MoveTowards(walkBlend, targetBlend, blendSpeed * Time.deltaTime);

        float frequency = Mathf.Lerp(idleBobFrequency, walkBobFrequency, walkBlend);
        phase += Time.deltaTime * frequency;

        float bob = Mathf.Sin(phase) * Mathf.Lerp(idleBobAmplitude, walkBobAmplitude, walkBlend);
        float pitch = Mathf.Sin(phase) * walkPitchDegrees * walkBlend;
        float roll = Mathf.Sin(phase * 0.5f) * walkRollDegrees * walkBlend;

        visualRoot.localPosition = baseLocalPosition + Vector3.up * bob;
        visualRoot.localRotation = baseLocalRotation * Quaternion.Euler(pitch, 0f, roll);
    }

    private float GetPlanarSpeed()
    {
        if (parentRigidbody == null)
            return 0f;

#if UNITY_6000_0_OR_NEWER
        Vector3 velocity = parentRigidbody.linearVelocity;
#else
        Vector3 velocity = parentRigidbody.velocity;
#endif
        velocity.y = 0f;
        return velocity.magnitude;
    }
}
