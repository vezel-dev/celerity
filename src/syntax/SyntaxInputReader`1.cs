namespace Vezel.Celerity.Syntax;

internal sealed class SyntaxInputReader<T>
{
    public int Count => _items.Count - _position;

    private readonly IReadOnlyList<T> _items;

    private int _position;

    public SyntaxInputReader(IReadOnlyList<T> items)
    {
        _items = items;
    }

    public (bool Success, T? First) Peek1()
    {
        return Count != 0 ? (true, _items[_position]) : default;
    }

    public (bool Success, T? First, T? Second) Peek2()
    {
        return Count >= 2 ? (true, _items[_position], _items[_position + 1]) : default;
    }

    public T Read()
    {
        return _items[_position++];
    }
}
