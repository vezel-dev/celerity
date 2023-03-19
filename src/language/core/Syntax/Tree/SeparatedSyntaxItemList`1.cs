using Vezel.Celerity.Language.Text;

namespace Vezel.Celerity.Language.Syntax.Tree;

[SuppressMessage("", "CA1815")]
public readonly struct SeparatedSyntaxItemList<T> : IReadOnlyList<SyntaxItem>
    where T : SyntaxItem
{
    public struct Enumerator : IEnumerator<SyntaxItem>
    {
        private readonly SeparatedSyntaxItemList<T> _list;

        private int _index = -1;

        public readonly SyntaxItem Current => _list[_index];

        readonly object IEnumerator.Current => Current;

        internal Enumerator(SeparatedSyntaxItemList<T> list)
        {
            _list = list;
        }

        readonly void IDisposable.Dispose()
        {
        }

        public bool MoveNext()
        {
            return ++_index < _list.Count;
        }

        readonly void IEnumerator.Reset()
        {
            throw new NotSupportedException();
        }
    }

    public SyntaxTree Tree => Parent.Tree;

    public SyntaxItem Parent { get; }

    public SourceTextSpan Span { get; }

    public SourceTextSpan FullSpan { get; }

    public ImmutableArray<T> Elements { get; }

    public ImmutableArray<SyntaxToken> Separators { get; }

    public bool IsDefault => Elements.IsDefault;

    public int Count => Elements.Length + Separators.Length;

    public bool IsEmpty => Count == 0;

    public bool IsDefaultOrEmpty => Elements.IsDefaultOrEmpty;

    public SyntaxItem this[int index]
    {
        get
        {
            var (idx, sep) = int.DivRem(index, 2);

            return sep == 0 ? Elements[idx] : Separators[idx];
        }
    }

    // This constructs a partially-initialized list. It is only intended for use in LanguageParser.
    internal SeparatedSyntaxItemList(ImmutableArray<T> elements, ImmutableArray<SyntaxToken> separators)
        : this(elements, separators, null!)
    {
    }

    // This constructs a fully-initialized list (with Parent and Span/FullSpan set). Used by generated node classes.
    internal SeparatedSyntaxItemList(SeparatedSyntaxItemList<T> list, SyntaxItem parent)
        : this(list.Elements, list.Separators, parent)
    {
    }

    private SeparatedSyntaxItemList(
        ImmutableArray<T> elements, ImmutableArray<SyntaxToken> separators, SyntaxItem parent)
    {
        Parent = parent;
        Elements = elements;
        Separators = separators;

        if (parent == null)
            return;

        // There cannot be separators without elements.
        if (!elements.IsEmpty)
        {
            var first = elements[0];
            var last = (SyntaxItem)(separators.Length == elements.Length ? separators[^1] : elements[^1]);

            var firstSpan = first.Span;
            var firstFullSpan = first.FullSpan;

            Span = new(firstSpan.Start, last.Span.End - firstSpan.Start);
            FullSpan = new(firstFullSpan.Start, last.FullSpan.End - firstFullSpan.Start);
        }

        foreach (var element in elements)
            element.SetParent(parent);

        foreach (var separator in separators)
            separator.SetParent(parent);
    }

    public Enumerator GetEnumerator()
    {
        return new(this);
    }

    IEnumerator<SyntaxItem> IEnumerable<SyntaxItem>.GetEnumerator()
    {
        return GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public SourceTextLocation GetLocation()
    {
        return Tree.GetText().GetLocation(Span);
    }

    public SourceTextLocation GetFullLocation()
    {
        return Tree.GetText().GetLocation(FullSpan);
    }

    public SourceText GetText()
    {
        return new StringSourceText(Tree.Path, ToString());
    }

    public SourceText GetFullText()
    {
        return new StringSourceText(Tree.Path, ToFullString());
    }

    public override string ToString()
    {
        return ToString(false);
    }

    public string ToFullString()
    {
        return ToString(true);
    }

    private string ToString(bool full)
    {
        if (IsEmpty)
            return string.Empty;

        var sb = new StringBuilder();

        var first = this[0];
        var last = this[^1];

        foreach (var item in this)
            item.ToString(sb, full || item != first, full || item != last);

        return sb.ToString();
    }
}
