using UnityEngine;

/// <summary>
/// Точка, куда переносится игрок при входе в комнату.
/// Нужна как удобный маркер в сцене.
/// </summary>
public sealed class RoomSpawnPoint : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position, 0.25f);
        Gizmos.DrawLine(transform.position, transform.position + transform.forward);
    }
}