namespace Vezel.Celerity.Syntax;

internal sealed class SyntaxInputReader<T>
{
    public readonly struct Mark
    {
        public int Position { get; }

        public Mark(int position)
        {
            Position = position;
        }
    }

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

    public (bool Success, T? First, T? Second, T? Third) Peek3()
    {
        return Count >= 3 ? (true, _items[_position], _items[_position + 1], _items[_position + 2]) : default;
    }

    public T Read()
    {
        return _items[_position++];
    }

    // These should be used very sparingly. They were mainly intended for dealing with attributes during parsing.

    public Mark Save()
    {
        return new(_position);
    }

    public void Rewind(Mark mark)
    {
        _position = mark.Position;
    }
}
