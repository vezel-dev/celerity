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

    public RootScope GetRoot()
    {
        // RootScope is the only scope with a null Parent.
        return Parent != null ? Parent.GetRoot() : Unsafe.As<RootScope>(this);
    }

    public virtual LoopScope? GetEnclosingLoop()
    {
        return this is LoopScope loop ? loop : Parent?.GetEnclosingLoop();
    }

    private protected bool TryDefineSymbol(Symbol symbol, [MaybeNullWhen(true)] out Symbol existing)
    {
        if (Symbols.TryGetValue(symbol.Name, out existing))
            return false;

        // TODO: Use a collection builder behind the scenes.
        Symbols = Symbols.Add(symbol.Name, symbol);

        return true;
    }

    public Symbol? ResolveSymbol(string name)
    {
        return Symbols.TryGetValue(name, out var sym) ? sym : Parent?.ResolveSymbol(name);
    }

    internal virtual Symbol? ResolveOrCaptureSymbol(string name)
    {
        return ResolveSymbol(name);
    }
}
