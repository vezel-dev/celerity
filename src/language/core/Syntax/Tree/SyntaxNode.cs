using Vezel.Celerity.Language.Text;

namespace Vezel.Celerity.Language.Syntax.Tree;

public abstract class SyntaxNode : SyntaxItem
{
    public new SyntaxNode? Parent => Unsafe.As<SyntaxNode?>(base.Parent);

    public override SourceTextSpan Span
    {
        get
        {
            EnsureSpansInitialized(out var span, out _);

            return span;
        }
    }

    public override SourceTextSpan FullSpan
    {
        get
        {
            EnsureSpansInitialized(out _, out var fullSpan);

            return fullSpan;
        }
    }

    public override bool HasChildren => HasNodes || HasTokens;

    public abstract bool HasNodes { get; }

    public abstract bool HasTokens { get; }

    private volatile bool _spansInitialized;

    private SourceTextSpan _span;

    private SourceTextSpan _fullSpan;

    private protected SyntaxNode()
    {
    }

    private void EnsureSpansInitialized(out SourceTextSpan span, out SourceTextSpan fullSpan)
    {
        if (_spansInitialized)
        {
            span = _span;
            fullSpan = _fullSpan;

            return;
        }

        var tokens = DescendantTokens().ToArray();
        var first = tokens[0];

        // With the way the parser works today, a node's first token is either present or all of its tokens are missing;
        // there is never a situation where the first token is missing but the rest are present.
        if (!first.IsMissing)
        {
            var firstSpan = first.Span;
            var firstFullSpan = first.FullSpan;

            // We are only interested in the last token that is actually present.
            var last = tokens.Last(tok => !tok.IsMissing);

            span = new(firstSpan.Start, last.Span.End - firstSpan.Start);
            fullSpan = new(firstFullSpan.Start, last.FullSpan.End - firstFullSpan.Start);
        }
        else
            span = fullSpan = SourceTextSpan.Empty;

        _span = span;
        _fullSpan = fullSpan;

        _spansInitialized = true;
    }

    public new IEnumerable<SyntaxNode> Ancestors()
    {
        return base.Ancestors().UnsafeCast<SyntaxNode>();
    }

    public new IEnumerable<SyntaxNode> AncestorsAndSelf()
    {
        return base.AncestorsAndSelf().UnsafeCast<SyntaxNode>();
    }

    public new IEnumerable<SyntaxNode> Siblings()
    {
        return base.Siblings().UnsafeCast<SyntaxNode>();
    }

    public new IEnumerable<SyntaxNode> SiblingsAndSelf()
    {
        return base.SiblingsAndSelf().UnsafeCast<SyntaxNode>();
    }

    public abstract IEnumerable<SyntaxNode> ChildNodes();

    public abstract IEnumerable<SyntaxToken> ChildTokens();

    public override IEnumerable<SyntaxItem> Descendants()
    {
        var work = new Stack<SyntaxItem>();

        work.Push(this);

        do
        {
            var current = work.Pop();

            if (current != this)
                yield return current;

            if (current.HasChildren)
                foreach (var child in current.Children().Reverse())
                    work.Push(child);
        }
        while (work.Count != 0);
    }

    public IEnumerable<SyntaxNode> DescendantNodes()
    {
        var work = new Stack<SyntaxNode>();

        work.Push(this);

        do
        {
            var current = work.Pop();

            if (current != this)
                yield return current;

            if (current.HasNodes)
                foreach (var child in current.ChildNodes().Reverse())
                    work.Push(child);
        }
        while (work.Count != 0);
    }

    public IEnumerable<SyntaxToken> DescendantTokens()
    {
        var work = new Stack<SyntaxItem>();

        work.Push(this);

        do
        {
            var current = work.Pop();

            // Only yield tokens, and avoid descending into trivia (which would allocate unnecessary enumerators).
            if (current is SyntaxToken token)
                yield return token;
            else if (current is SyntaxNode { HasChildren: true })
                foreach (var child in current.Children().Reverse())
                    work.Push(child);
        }
        while (work.Count != 0);
    }

    public IEnumerable<SyntaxTrivia> DescendantTrivia()
    {
        // Avoid enumerator allocations by explicitly accessing the trivia on tokens.
        foreach (var token in DescendantTokens())
        {
            foreach (var leading in token.LeadingTrivia)
                yield return leading;

            foreach (var trailing in token.TrailingTrivia)
                yield return trailing;
        }
    }

    internal abstract void Visit(SyntaxVisitor visitor);

    internal abstract T? Visit<T>(SyntaxVisitor<T> visitor);
}
