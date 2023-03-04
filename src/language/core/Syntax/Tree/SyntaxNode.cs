using Vezel.Celerity.Language.Text;

namespace Vezel.Celerity.Language.Syntax.Tree;

public abstract class SyntaxNode : SyntaxItem
{
    public new SyntaxNode? Parent => Unsafe.As<SyntaxNode?>(base.Parent);

    public override sealed SourceTextSpan Span => EnsureSpansInitialized().Span;

    public override sealed SourceTextSpan FullSpan => EnsureSpansInitialized().FullSpan;

    public override sealed bool HasChildren => HasNodes || HasTokens;

    public abstract bool HasNodes { get; }

    public abstract bool HasTokens { get; }

    private volatile bool _spansInitialized;

    private SourceTextSpan _span;

    private SourceTextSpan _fullSpan;

    private protected SyntaxNode()
    {
    }

    private (SourceTextSpan Span, SourceTextSpan FullSpan) EnsureSpansInitialized()
    {
        if (_spansInitialized)
            return (_span, _fullSpan);

        var tokens = DescendantTokens().ToArray();
        var first = tokens.FirstOrDefault(tok => !tok.IsMissing);

        SourceTextSpan span;
        SourceTextSpan fullSpan;

        if (first != null)
        {
            // This could end up being the same as the first token.
            var last = tokens.Last(tok => !tok.IsMissing);

            var firstSpan = first.Span;
            var firstFullSpan = first.FullSpan;

            span = new(firstSpan.Start, last.Span.End - firstSpan.Start);
            fullSpan = new(firstFullSpan.Start, last.FullSpan.End - firstFullSpan.Start);
        }
        else
            span = fullSpan = default;

        _span = span;
        _fullSpan = fullSpan;

        _spansInitialized = true;

        return (span, fullSpan);
    }

    public new IEnumerable<SyntaxNode> Ancestors()
    {
        return base.Ancestors().UnsafeCast<SyntaxNode>();
    }

    public new IEnumerable<SyntaxNode> AncestorsAndSelf()
    {
        return base.AncestorsAndSelf().UnsafeCast<SyntaxNode>();
    }

    public abstract IEnumerable<SyntaxNode> ChildNodes();

    public IEnumerable<SyntaxNode> ChildNodesAndSelf()
    {
        yield return this;

        foreach (var child in ChildNodes())
            yield return child;
    }

    public abstract IEnumerable<SyntaxToken> ChildTokens();

    public override sealed IEnumerable<SyntaxItem> Descendants()
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

    public IEnumerable<SyntaxNode> DescendantNodesAndSelf()
    {
        yield return this;

        foreach (var descendant in DescendantNodes())
            yield return descendant;
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
        foreach (var token in DescendantTokens())
        {
            // Avoid enumerator allocations by explicitly accessing the trivia on tokens.

            foreach (var leading in token.LeadingTrivia)
                yield return leading;

            foreach (var trailing in token.TrailingTrivia)
                yield return trailing;
        }
    }

    internal abstract void Visit(SyntaxVisitor visitor);

    internal abstract T? Visit<T>(SyntaxVisitor<T> visitor);

    public override sealed string ToString()
    {
        return ToString(false);
    }

    public override sealed string ToFullString()
    {
        return ToString(true);
    }

    private string ToString(bool full)
    {
        var sb = new StringBuilder();

        var tokens = DescendantTokens().ToArray();
        var first = tokens.FirstOrDefault(tok => !tok.IsMissing);

        if (first == null)
            return string.Empty;

        // This could end up being the same as the first token.
        var last = tokens.LastOrDefault(tok => !tok.IsMissing);

        foreach (var token in tokens)
        {
            if (token.IsMissing)
                continue;

            if (token != first || full)
                foreach (var leading in token.LeadingTrivia)
                    _ = sb.Append(leading);

            _ = sb.Append(token);

            if (token != last || full)
                foreach (var trailing in token.TrailingTrivia)
                    _ = sb.Append(trailing);
        }

        return sb.ToString();
    }
}
