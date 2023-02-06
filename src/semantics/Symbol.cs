namespace Vezel.Celerity.Semantics;

public abstract class Symbol
{
    public SyntaxNode Syntax { get; }

    public Scope Scope { get; }

    public abstract string Name { get; }

    public virtual bool IsMutable => false;

    private protected Symbol(SyntaxNode syntax, Scope scope)
    {
        Syntax = syntax;
        Scope = scope;
    }
}
