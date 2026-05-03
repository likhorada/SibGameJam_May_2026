using UnityEngine;

/// <summary>
/// Управляет связанными парами: Позиция спавна + Камера.
/// Переключается циклически по событию.
/// </summary>
public class SpawnAndCameraSwitcher : MonoBehaviour
{
    [System.Serializable]
    public class Slot
    {
        [Tooltip("Точка спавна персонажа (Transform)")]
        public Transform spawnPoint;
        
        [Tooltip("Компонент Camera. Скрипт будет включать/выключать GameObject, на котором он висит.")]
        public Camera camera;
    }

    [Header("Слоты (Позиция + Камера)")]
    public Slot[] slots = new Slot[3];

    [Header("Персонаж")]
    public Transform player;

    private int currentIndex = 0;

    private void Start()
    {
        Debug.Log("Start");
        ApplyState(currentIndex);
    }

    /// <summary> Вызывается при наступлении события (UI, триггер, анимация и т.д.) </summary>
    public void TriggerEvent()
    {
        currentIndex = (currentIndex + 1) % slots.Length;
        ApplyState(currentIndex);
    }

    /// <summary> Принудительное переключение на конкретный индекс (0, 1, 2...) </summary>
    public void SwitchToIndex(int index)
    {
        if (index < 0 || index >= slots.Length)
        {
            Debug.LogError($"Индекс вне диапазона. Допустимо: 0-{slots.Length - 1}");
            return;
        }
        currentIndex = index;
        ApplyState(index);
    }

    private void ApplyState(int index)
    {
        if (index < 0 || index >= slots.Length || slots[index] == null) return;
        var current = slots[index];
        if (player != null && current.spawnPoint != null)
        {
            if (player.TryGetComponent<Rigidbody>(out var rb))
            {
                rb.position = current.spawnPoint.position;
                rb.rotation = current.spawnPoint.rotation;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            else
            {
                player.position = current.spawnPoint.position;
                player.rotation = current.spawnPoint.rotation;
            }
        }

        // 2. Переключение камер
        for (int i = 0; i < slots.Length; i++)
        {
            var slot = slots[i];
            if (slot == null || slot.camera == null) continue;

            bool shouldBeActive = (i == index);
            if (slot.camera.gameObject.activeSelf != shouldBeActive)
            {
                slot.camera.gameObject.SetActive(shouldBeActive);
            }
        }
    }

    // Для быстрого теста прямо в редакторе
    [ContextMenu("🔹 Тестовый вызов события")]
    private void TestTrigger() => TriggerEvent();
}