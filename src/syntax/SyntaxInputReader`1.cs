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

    public bool IsEmpty => _position == _items.Count;

    private readonly IReadOnlyList<T> _items;

    private int _position;

    public SyntaxInputReader(IReadOnlyList<T> items)
    {
        _items = items;
    }

    public bool TryPeek(int offset, [MaybeNullWhen(false)] out T item)
    {
        var position = _position + offset;

        if (position < _items.Count)
        {
            item = _items[position];

            return true;
        }

        item = default;

        return false;
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
