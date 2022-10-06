namespace Vezel.Celerity.Syntax;

[SuppressMessage("", "CA1815")]
public readonly struct SyntaxItemList<T> : IReadOnlyList<T>
    where T : SyntaxItem
{
    public int Count => _items.Length;

    public T this[int index] => _items[index];

    private readonly ImmutableArray<T> _items;

    internal SyntaxItemList(ImmutableArray<T> items)
    {
        _items = items;
    }

    public ImmutableArray<T>.Enumerator GetEnumerator()
    {
        return _items.GetEnumerator();
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return _items.AsEnumerable().GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _items.AsEnumerable().GetEnumerator();
    }
}
