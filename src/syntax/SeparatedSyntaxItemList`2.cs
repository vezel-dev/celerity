namespace Vezel.Celerity.Syntax;

[SuppressMessage("", "CA1710")]
[SuppressMessage("", "CA1815")]
public readonly struct SeparatedSyntaxItemList<TItem, TSeparator> : IReadOnlyCollection<SyntaxItem>
    where TItem : SyntaxItem
    where TSeparator : SyntaxItem
{
    public struct Enumerator : IEnumerator<SyntaxItem>
    {
        private enum State
        {
            Item,
            Separator,
        }

        private SyntaxItemList<TItem>.Enumerator _items;

        private SyntaxItemList<TSeparator>.Enumerator _separators;

        private State _state;

        private SyntaxItem? _current;

        public SyntaxItem Current => _current ?? throw new InvalidOperationException();

        object IEnumerator.Current => Current;

        internal Enumerator(SyntaxItemList<TItem>.Enumerator items, SyntaxItemList<TSeparator>.Enumerator separators)
        {
            _items = items;
            _separators = separators;
        }

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            // There will always be a separator between any two items, but the last item may have a dangling separator.
            switch (_state)
            {
                case State.Item when _items.MoveNext():
                    _state = State.Separator;
                    _current = _items.Current;

                    return true;
                case State.Separator when _separators.MoveNext():
                    _state = State.Item;
                    _current = _separators.Current;

                    return true;
                default:
                    _current = null;

                    return false;
            }
        }

        public void Reset()
        {
            throw new NotSupportedException();
        }
    }

    public SyntaxItemList<TItem> Items { get; }

    public SyntaxItemList<TSeparator> Separators { get; }

    public int Count => Items.Count + Separators.Count;

    internal SeparatedSyntaxItemList(SyntaxItemList<TItem> items, SyntaxItemList<TSeparator> separators)
    {
        Items = items;
        Separators = separators;
    }

    public Enumerator GetEnumerator()
    {
        return new(Items.GetEnumerator(), Separators.GetEnumerator());
    }

    IEnumerator<SyntaxItem> IEnumerable<SyntaxItem>.GetEnumerator()
    {
        return GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
