namespace Vezel.Celerity.Syntax;

public abstract class SyntaxNode : SyntaxItem
{
    public override SyntaxNode? Parent => _parent;

    public abstract bool HasTokens { get; }

    public abstract bool HasChildren { get; }

    private SyntaxNode? _parent;

    private protected SyntaxNode()
    {
    }

    internal void SetParent(SyntaxNode parent)
    {
        _parent = parent;
    }

    public abstract IEnumerable<SyntaxItem> Items();

    public IEnumerable<SyntaxToken> Tokens()
    {
        return Items().OfType<SyntaxToken>();
    }

    public IEnumerable<SyntaxNode> Children()
    {
        return Items().OfType<SyntaxNode>();
    }

    internal abstract T Visit<T>(SyntaxWalker<T> walker, T state);
}
