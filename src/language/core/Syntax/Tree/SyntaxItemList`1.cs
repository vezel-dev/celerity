// SPDX-License-Identifier: 0BSD

using Vezel.Celerity.Language.Text;

namespace Vezel.Celerity.Language.Syntax.Tree;

[SuppressMessage("", "CA1815")]
public readonly struct SyntaxItemList<T> : IReadOnlyList<T>
    where T : SyntaxItem
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

        void IEnumerator.Reset()
        {
            throw new NotSupportedException();
        }
    }

    public SyntaxTree Tree => Parent.Tree;

    public SyntaxItem Parent { get; }

    public SourceTextSpan Span { get; }

    public SourceTextSpan FullSpan { get; }

    public bool IsDefault => _items.IsDefault;

    public int Count => _items.Length;

    public bool IsEmpty => _items.IsEmpty;

    public bool IsDefaultOrEmpty => _items.IsDefaultOrEmpty;

    public T this[int index] => _items[index];

    private readonly ImmutableArray<T> _items;

    // This constructs a partially-initialized list. It is only intended for use in LanguageParser.
    internal SyntaxItemList(ImmutableArray<T> items)
        : this(items, parent: null!)
    {
    }

    // This constructs a fully-initialized list (with Parent and Span/FullSpan set). Used by generated node classes.
    internal SyntaxItemList(SyntaxItemList<T> list, SyntaxItem parent)
        : this(list._items, parent)
    {
    }

    private SyntaxItemList(ImmutableArray<T> items, SyntaxItem parent)
    {
        Parent = parent;
        _items = items;

        if (parent == null)
            return;

        if (!items.IsEmpty)
        {
            var first = items[0];
            var last = items[^1];

            Span = SourceTextSpan.Union(first.Span, last.Span);
            FullSpan = SourceTextSpan.Union(first.FullSpan, last.FullSpan);
        }

        foreach (var item in items)
            item.SetParent(parent);
    }

    internal ImmutableArray<T> AsImmutableArray()
    {
        return _items;
    }

    [SuppressMessage("", "RS0041")] // TODO: https://github.com/dotnet/roslyn-analyzers/issues/6921
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
        return ToString(full: false);
    }

    public string ToFullString()
    {
        return ToString(full: true);
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
