using System;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider))]
public class TriggerEventOnBoxEnter : MonoBehaviour
{
    [Header("Кого уведомлять")]
    [SerializeField] private UnityEvent onEnter = new UnityEvent();

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

    private void OnTriggerEnter(Collider other)
    {
        if (_wasTriggered && singleUse) return;

        if (onlyPlayerTag && !other.CompareTag(playerTag)) return;

        onEnter?.Invoke();
        _wasTriggered = true;
    }
}
