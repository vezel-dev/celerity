namespace Vezel.Celerity.Language.Syntax.Tree;

[SuppressMessage("", "CA1815")]
public readonly struct SeparatedSyntaxItemList<TElement, TSeparator> : IReadOnlyList<SyntaxItem>
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

        public readonly SyntaxItem Current => _current ?? throw new InvalidOperationException();

        readonly object IEnumerator.Current => Current;

        internal Enumerator(
            SyntaxItemList<TElement>.Enumerator elements, SyntaxItemList<TSeparator>.Enumerator separators)
        {
            _elements = elements;
            _separators = separators;
        }

        readonly void IDisposable.Dispose()
        {
        }

        public bool MoveNext()
        {
            bool result;

            // There will always be a separator between any two elements, but the last element may have a dangling
            // separator.
            (_state, _current, result) = _state switch
            {
                State.Element when _elements.MoveNext() => (State.Separator, _elements.Current, true),
                State.Separator when _separators.MoveNext() => (State.Element, _separators.Current, true),
                _ => (_state, default(SyntaxItem), false),
            };

            return result;
        }

        readonly void IEnumerator.Reset()
        {
            throw new NotSupportedException();
        }
    }

    public static SeparatedSyntaxItemList<TElement, TSeparator> Empty { get; } =
        new(SyntaxItemList<TElement>.Empty, SyntaxItemList<TSeparator>.Empty);

    public SyntaxItemList<TElement> Elements { get; }

    public SyntaxItemList<TSeparator> Separators { get; }

    public bool IsDefault => Elements.IsDefault;

    public int Count => Elements.Count + Separators.Count;

    public bool IsEmpty => Count == 0;

    public bool IsDefaultOrEmpty => Elements.IsDefaultOrEmpty;

    public SyntaxItem this[int index] => index % 2 == 0 ? Elements[index] : Separators[index];

    internal SeparatedSyntaxItemList(SyntaxItemList<TElement> elements, SyntaxItemList<TSeparator> separators)
    {
        Elements = elements;
        Separators = separators;
    }

    public static implicit operator SeparatedSyntaxItemList<TElement, SyntaxItem>(
        SeparatedSyntaxItemList<TElement, TSeparator> list) => new(list.Elements, list.Separators);

    public static implicit operator SeparatedSyntaxItemList<SyntaxItem, TSeparator>(
        SeparatedSyntaxItemList<TElement, TSeparator> list) => new(list.Elements, list.Separators);

    public static implicit operator SeparatedSyntaxItemList<SyntaxItem, SyntaxItem>(
        SeparatedSyntaxItemList<TElement, TSeparator> list) => new(list.Elements, list.Separators);

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
