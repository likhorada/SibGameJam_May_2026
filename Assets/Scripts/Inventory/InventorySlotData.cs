/// <summary>
/// Данные одного слота инвентаря. Один слот хранит только один элемент.
/// </summary>
public sealed class InventorySlotData
{
    public ElementKind ElementId { get; private set; }

    public string ElementName
    {
        get { return ElementCatalog.GetDisplayName(ElementId); }
    }

    public bool IsEmpty
    {
        get { return ElementId == ElementKind.None; }
    }

    public void Set(ElementKind elementId)
    {
        ElementId = elementId;
    }

    public void Clear()
    {
        ElementId = ElementKind.None;
    }
}