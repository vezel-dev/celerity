namespace Vezel.Celerity.Syntax;

internal sealed class SyntaxInputReader<T>
{
    private ReadOnlyMemory<T> _items;

    public SyntaxInputReader(ReadOnlyMemory<T> items)
    {
        _items = items;
    }

    public bool TryPeek(out T item1)
    {
        if (_items.IsEmpty)
        {
            Unsafe.SkipInit(out item1);

            return false;
        }

        item1 = _items.Span[0];

        return true;
    }

    public bool TryPeek(out T item1, out T item2)
    {
        if (_items.IsEmpty)
        {
            Unsafe.SkipInit(out item1);
            Unsafe.SkipInit(out item2);

            return false;
        }

        item1 = _items.Span[0];
        item2 = _items.Span[1];

        return true;
    }

    public T Read()
    {
        var item = _items.Span[0];

        _items = _items[1..];

        return item;
    }
}
