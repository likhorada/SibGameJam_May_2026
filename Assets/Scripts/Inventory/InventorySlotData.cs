/// <summary>
/// Данные одного слота инвентаря.
/// Один слот хранит только один элемент.
/// </summary>
public sealed class InventorySlotData
{
    public ElementDefinition Element { get; private set; }

    public bool IsEmpty
    {
        get { return Element == null; }
    }

    public void Set(ElementDefinition element)
    {
        Element = element;
    }

    public void Clear()
    {
        Element = null;
    }
}