namespace Vezel.Celerity.Syntax;

public abstract class SyntaxNode : SyntaxItem
{
    public new SyntaxNode? Parent => Unsafe.As<SyntaxNode?>(base.Parent);

    public abstract bool HasTokens { get; }

    public abstract bool HasChildren { get; }

    private protected SyntaxNode()
    {
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
