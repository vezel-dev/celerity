namespace Vezel.Celerity.Semantics;

public abstract class Symbol
{
    public SyntaxNode Syntax { get; }

    public abstract string Name { get; }

    public virtual bool IsMutable => false;

    private protected Symbol(SyntaxNode syntax)
    {
        Syntax = syntax;
    }
}
