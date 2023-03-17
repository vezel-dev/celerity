namespace Vezel.Celerity.Buffers;

internal sealed class ListReader<T>
{
    public readonly struct Mark
    {
        public int Position { get; }

        public Mark(int position)
        {
            Position = position;
        }
    }

    public int Position { get; private set; }

    public bool IsEmpty => Position == _items.Count;

    private readonly IReadOnlyList<T> _items;

    public ListReader(IReadOnlyList<T> items)
    {
        _items = items;
    }

    public bool TryPeek(int offset, [MaybeNullWhen(false)] out T item)
    {
        var position = Position + offset;

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
        return _items[Position++];
    }

    public Mark Save()
    {
        return new(Position);
    }

    public void Rewind(Mark mark)
    {
        Position = mark.Position;
    }
}
