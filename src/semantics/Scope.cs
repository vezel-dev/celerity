namespace Vezel.Celerity.Semantics;

public abstract class Scope
{
    public SyntaxNode Syntax { get; }

    public Scope? Parent { get; }

    public ImmutableDictionary<string, Symbol> Symbols { get; private set; } =
        ImmutableDictionary<string, Symbol>.Empty;

    private protected Scope(SyntaxNode syntax, Scope? parent)
    {
        Syntax = syntax;
        Parent = parent;
    }

    internal void Define(Symbol symbol)
    {
        Symbols = Symbols.Add(symbol.Name, symbol);
    }
}
