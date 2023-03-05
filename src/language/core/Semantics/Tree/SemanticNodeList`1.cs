namespace Vezel.Celerity.Language.Semantics.Tree;

[SuppressMessage("", "CA1815")]
public readonly struct SemanticNodeList<T> : IReadOnlyList<T>
    where T : SemanticNode
{
    public struct Enumerator : IEnumerator<T>
    {
        public readonly T Current => _enumerator.Current;

        readonly object IEnumerator.Current => Current;

        private ImmutableArray<T>.Enumerator _enumerator;

        internal Enumerator(ImmutableArray<T>.Enumerator enumerator)
        {
            _enumerator = enumerator;
        }

        readonly void IDisposable.Dispose()
        {
        }

        public bool MoveNext()
        {
            return _enumerator.MoveNext();
        }

        readonly void IEnumerator.Reset()
        {
            throw new NotSupportedException();
        }
    }

    public static SemanticNodeList<T> Empty { get; } = new(ImmutableArray<T>.Empty);

    public bool IsDefault => _items.IsDefault;

    public int Count => _items.Length;

    public bool IsEmpty => _items.IsEmpty;

    public bool IsDefaultOrEmpty => _items.IsDefaultOrEmpty;

    public T this[int index] => _items[index];

    private readonly ImmutableArray<T> _items;

    internal SemanticNodeList(ImmutableArray<T> items)
    {
        _items = items;
    }

    public static implicit operator SemanticNodeList<SemanticNode>(SemanticNodeList<T> list) =>
        new(ImmutableArray<SemanticNode>.CastUp(list._items));

    public Enumerator GetEnumerator()
    {
        return new(_items.GetEnumerator());
    }

    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
