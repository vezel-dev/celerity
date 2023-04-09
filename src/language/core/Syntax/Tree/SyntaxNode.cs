using Vezel.Celerity.Language.Text;

namespace Vezel.Celerity.Language.Syntax.Tree;

public abstract class SyntaxNode : SyntaxItem
{
    public new SyntaxNode? Parent => Unsafe.As<SyntaxNode?>(base.Parent);

    public override sealed SourceTextSpan Span => _span;

    public override sealed SourceTextSpan FullSpan => _fullSpan;

    public override sealed bool HasChildren => HasNodes || HasTokens;

    public abstract bool HasNodes { get; }

    public abstract bool HasTokens { get; }

    private SourceTextSpan _span;

    private SourceTextSpan _fullSpan;

    private protected SyntaxNode()
    {
    }

    // Called by constructors in generated node classes.
    private protected void Initialize()
    {
        if (!HasChildren)
            return;

        var children = Children().ToArray();
        var first = children[0];
        var last = children[^1];

        _span = SourceTextSpan.Union(first.Span, last.Span);
        _fullSpan = SourceTextSpan.Union(first.FullSpan, last.FullSpan);
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

    public IEnumerable<SyntaxTerminal> DescendantTerminals()
    {
        foreach (var token in DescendantTokens())
        {
            // Avoid enumerator allocations by explicitly accessing the trivia on tokens.

            foreach (var leading in token.LeadingTrivia)
                yield return leading;

            yield return token;

            foreach (var trailing in token.TrailingTrivia)
                yield return trailing;
        }
    }

    internal abstract void Visit(SyntaxVisitor visitor);

    internal abstract T? Visit<T>(SyntaxVisitor<T> visitor);

    public override sealed string ToString()
    {
        return base.ToString();
    }

    public override sealed string ToFullString()
    {
        return base.ToFullString();
    }

    internal override void ToString(StringBuilder builder, bool leading, bool trailing)
    {
        if (!HasChildren)
            return;

        var children = Children().ToArray();
        var first = children[0];
        var last = children[^1];

        foreach (var child in children)
            child.ToString(builder, leading || child != first, trailing || child != last);
    }
}
