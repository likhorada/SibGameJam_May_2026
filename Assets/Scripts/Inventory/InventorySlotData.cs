/// <summary>
/// Данные одного слота инвентаря. Один слот хранит только один элемент.
/// </summary>
public sealed class InventorySlotData
{
    public string ElementId { get; private set; }
    public string ElementName { get; private set; }

    public bool IsEmpty
    {
        get { return string.IsNullOrEmpty(ElementId); }
    }

    public void Set(string elementId, string elementName)
    {
        ElementId = elementId;
        ElementName = elementName;
    }

    public void Clear()
    {
        ElementId = string.Empty;
        ElementName = string.Empty;
    }
}