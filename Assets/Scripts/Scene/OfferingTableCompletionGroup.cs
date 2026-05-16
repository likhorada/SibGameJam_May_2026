using UnityEngine;
using UnityEngine.Events;

public sealed class OfferingTableCompletionGroup : MonoBehaviour
{
    [SerializeField] private OfferingTableInteractable[] tables = new OfferingTableInteractable[4];
    [SerializeField] private bool autoFindTablesInChildren;
    [SerializeField] private bool invokeIfAlreadyCompleteOnStart = true;
    [SerializeField] private bool invokeOnlyOnce = true;
    [SerializeField] private UnityEvent onAllCompleted = new UnityEvent();

    private bool completed;

    public bool IsCompleted
    {
        get { return completed || AreAllTablesComplete(); }
    }

    private void OnEnable()
    {
        RefreshAutoFoundTables();
        Subscribe();
    }

    private void Start()
    {
        if (invokeIfAlreadyCompleteOnStart)
            Evaluate();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    public void Evaluate()
    {
        if (invokeOnlyOnce && completed)
            return;

        if (!AreAllTablesComplete())
            return;

        completed = true;
        onAllCompleted.Invoke();
    }

    public void ResetCompletionState()
    {
        completed = false;
    }

    private void HandleTableCompleted()
    {
        Evaluate();
    }

    private bool AreAllTablesComplete()
    {
        if (tables == null || tables.Length == 0)
            return false;

        for (int i = 0; i < tables.Length; i++)
        {
            if (tables[i] == null || !tables[i].IsComplete)
                return false;
        }

        return true;
    }

    private void RefreshAutoFoundTables()
    {
        if (!autoFindTablesInChildren)
            return;

        if (tables != null && tables.Length > 0)
            return;

        tables = GetComponentsInChildren<OfferingTableInteractable>(true);
    }

    private void Subscribe()
    {
        if (tables == null)
            return;

        for (int i = 0; i < tables.Length; i++)
        {
            if (tables[i] != null)
                tables[i].Completed += HandleTableCompleted;
        }
    }

    private void Unsubscribe()
    {
        if (tables == null)
            return;

        for (int i = 0; i < tables.Length; i++)
        {
            if (tables[i] != null)
                tables[i].Completed -= HandleTableCompleted;
        }
    }
}
