namespace Vezel.Celerity.Syntax.Tree;

[SuppressMessage("", "CA1710")]
[SuppressMessage("", "CA1815")]
public readonly struct SeparatedSyntaxItemList<TElement, TSeparator> : IReadOnlyCollection<SyntaxItem>
    where TElement : SyntaxItem
    where TSeparator : SyntaxItem
{
    public struct Enumerator : IEnumerator<SyntaxItem>
    {
        private enum State
        {
            Element,
            Separator,
        }

        private SyntaxItemList<TElement>.Enumerator _elements;

        private SyntaxItemList<TSeparator>.Enumerator _separators;

        private State _state;

        private SyntaxItem? _current;

        public SyntaxItem Current => _current ?? throw new InvalidOperationException();

        object IEnumerator.Current => Current;

        internal Enumerator(
            SyntaxItemList<TElement>.Enumerator elements, SyntaxItemList<TSeparator>.Enumerator separators)
        {
            _elements = elements;
            _separators = separators;
        }

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            // There will always be a separator between any two elements, but the last element may have a dangling
            // separator.
            switch (_state)
            {
                case State.Element when _elements.MoveNext():
                    _state = State.Separator;
                    _current = _elements.Current;

                    return true;
                case State.Separator when _separators.MoveNext():
                    _state = State.Element;
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

    public static SeparatedSyntaxItemList<TElement, TSeparator> Empty { get; } =
        new(SyntaxItemList<TElement>.Empty, SyntaxItemList<TSeparator>.Empty);

    public SyntaxItemList<TElement> Elements { get; }

    public SyntaxItemList<TSeparator> Separators { get; }

    public int Count => Elements.Count + Separators.Count;

    internal SeparatedSyntaxItemList(SyntaxItemList<TElement> elements, SyntaxItemList<TSeparator> separators)
    {
        Elements = elements;
        Separators = separators;
    }

    public Enumerator GetEnumerator()
    {
        return new(Elements.GetEnumerator(), Separators.GetEnumerator());
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
