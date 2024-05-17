// SPDX-License-Identifier: 0BSD

using Vezel.Celerity.Language.Syntax.Tree;

namespace Vezel.Celerity.Language.Semantics.Tree;

[SuppressMessage("", "CA1815")]
public readonly struct SemanticNodeList<TSemantics, TSyntax> : IReadOnlyList<TSemantics>
    where TSemantics : SemanticNode
    where TSyntax : SyntaxNode
{
    public struct Enumerator : IEnumerator<TSemantics>
    {
        public readonly TSemantics Current => _enumerator.Current;

        readonly object IEnumerator.Current => Current;

        private ImmutableArray<TSemantics>.Enumerator _enumerator;

        internal Enumerator(ImmutableArray<TSemantics>.Enumerator enumerator)
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

    public SyntaxItemList<TSyntax> Syntax { get; }

    public SemanticTree Tree => Parent.Tree;

    public SemanticNode Parent { get; }

    public bool IsDefault => _items.IsDefault;

    public int Count => _items.Length;

    public bool IsEmpty => _items.IsEmpty;

    public bool IsDefaultOrEmpty => _items.IsDefaultOrEmpty;

    public TSemantics this[int index] => _items[index];

    private readonly ImmutableArray<TSemantics> _items;

    // This constructs a partially-initialized list. It is only intended for use in LanguageAnalyzer.
    internal SemanticNodeList(SyntaxItemList<TSyntax> syntax, ImmutableArray<TSemantics> items)
        : this(syntax, items, parent: null!)
    {
    }

    // This constructs a fully-initialized list (with Parent set). Used by generated node classes.
    internal SemanticNodeList(SemanticNodeList<TSemantics, TSyntax> list, SemanticNode parent)
        : this(list.Syntax, list._items, parent)
    {
    }

    private SemanticNodeList(SyntaxItemList<TSyntax> syntax, ImmutableArray<TSemantics> items, SemanticNode parent)
    {
        Syntax = syntax;
        Parent = parent;
        _items = items;

        if (parent != null)
            foreach (var item in items)
                item.SetParent(parent);
    }

    [SuppressMessage("", "RS0041")] // TODO: https://github.com/dotnet/roslyn-analyzers/issues/6921
    public Enumerator GetEnumerator()
    {
        return new(_items.GetEnumerator());
    }

    IEnumerator<TSemantics> IEnumerable<TSemantics>.GetEnumerator()
    {
        return GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public override string ToString()
    {
        return Syntax.ToString();
    }

    public string ToFullString()
    {
        return Syntax.ToFullString();
    }
}
