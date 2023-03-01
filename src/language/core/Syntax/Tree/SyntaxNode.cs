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
            // We are only interested in the last token that is actually present.
            var last = tokens.Last(tok => !tok.IsMissing);

            var firstSpan = first.Span;
            var firstFullSpan = first.FullSpan;

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
        var work = new Queue<SyntaxItem>();

        work.Enqueue(this);

        do
        {
            var current = work.Dequeue();

            if (current.HasChildren)
                foreach (var child in current.Children())
                    work.Enqueue(child);

            yield return current;
        }
        while (work.Count != 0);
    }

    public IEnumerable<SyntaxNode> DescendantNodes()
    {
        var work = new Queue<SyntaxNode>();

        work.Enqueue(this);

        do
        {
            var current = work.Dequeue();

            if (current.HasNodes)
                foreach (var child in current.ChildNodes())
                    work.Enqueue(child);

            yield return current;
        }
        while (work.Count != 0);
    }

    public IEnumerable<SyntaxToken> DescendantTokens()
    {
        // Not much we can do better here.
        return Descendants().OfType<SyntaxToken>();
    }

    public IEnumerable<SyntaxTrivia> DescendantTrivia()
    {
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
