namespace Vezel.Celerity.Syntax;

public abstract partial class SyntaxWalker<T>
{
    private readonly SyntaxWalkerDepth _depth;

    protected SyntaxWalker(SyntaxWalkerDepth depth = SyntaxWalkerDepth.Nodes)
    {
        _ = Enum.IsDefined(depth) ? true : throw new ArgumentOutOfRangeException(nameof(depth));

        _depth = depth;
    }

    public T VisitNode(SyntaxNode node, T state)
    {
        ArgumentNullException.ThrowIfNull(node);

        return node.Visit(this, state);
    }

    protected virtual T DefaultVisitNode(SyntaxNode node, T state)
    {
        ArgumentNullException.ThrowIfNull(node);

        foreach (var elem in node.Items())
        {
            if (elem is SyntaxNode child)
                state = VisitNode(child, state);
            else if (_depth >= SyntaxWalkerDepth.Tokens)
                state = VisitToken(Unsafe.As<SyntaxToken>(elem), state);
        }

        return state;
    }

    public virtual T VisitToken(SyntaxToken token, T state)
    {
        ArgumentNullException.ThrowIfNull(token);

        if (_depth >= SyntaxWalkerDepth.Trivia)
        {
            if (token.HasLeadingTrivia)
                foreach (var trivia in token.LeadingTrivia)
                    state = VisitLeadingTrivia(trivia, state);

            if (token.HasTrailingTrivia)
                foreach (var trivia in token.TrailingTrivia)
                    state = VisitTrailingTrivia(trivia, state);
        }

        return state;
    }

    public virtual T VisitLeadingTrivia(SyntaxTrivia trivia, T state)
    {
        ArgumentNullException.ThrowIfNull(trivia);

        return DefaultVisitTrivia(trivia, state);
    }

    public virtual T VisitTrailingTrivia(SyntaxTrivia trivia, T state)
    {
        ArgumentNullException.ThrowIfNull(trivia);

        return DefaultVisitTrivia(trivia, state);
    }

    protected virtual T DefaultVisitTrivia(SyntaxTrivia trivia, T state)
    {
        ArgumentNullException.ThrowIfNull(trivia);

        return state;
    }
}
