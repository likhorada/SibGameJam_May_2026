using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class TriggerEventOnBoxEnter : MonoBehaviour
{
    [Header("Кого уведомлять")]
    [SerializeField] private SpawnAndCameraSwitcher switcher;

    [Header("Фильтр по объекту")]
    [SerializeField] private bool onlyPlayerTag = true;
    [SerializeField] private string playerTag = "Player";

    [Header("Поведение")]
    [SerializeField] private bool singleUse = true;

    private bool _wasTriggered;

    private void Reset()
    {
        var box = GetComponent<BoxCollider>();
        box.isTrigger = true;
    }

    private void Awake()
    {
        if (switcher == null)
        {
            switcher = FindFirstObjectByType<SpawnAndCameraSwitcher>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_wasTriggered && singleUse) return;
        if (switcher == null) return;

        if (onlyPlayerTag && !other.CompareTag(playerTag)) return;

        switcher.TriggerEvent();
        _wasTriggered = true;
    }
}
