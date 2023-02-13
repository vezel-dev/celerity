using Vezel.Celerity.Semantics.Tree;

namespace Vezel.Celerity.Semantics.Binding;

internal class Scope
{
    public Scope? Parent { get; }

    private readonly Dictionary<string, Symbol> _symbols = new();

    public Scope(Scope? parent)
    {
        Parent = parent;
    }

    public virtual LoopScope? GetEnclosingLoop()
    {
        // We might be analyzing a bizarre piece of code like this:
        //
        // while true {
        //     while break {
        //     };
        // };
        //
        // In this case, when we get to the break expression, the top of the scope stack will be a LoopScope for the
        // inner while expression, but we want to bind the break expression to the LoopScope of the outer while
        // expression. So we look at Parent here, rather than this instance.
        //
        // Note the following:
        //
        // while true {
        //     break;
        // };
        //
        // Here, when we get to the break expression, the top of the scope stack will be the BlockScope, whose parent
        // will be the LoopScope for the while expression. So this case is handled correctly as well.
        return Parent is LoopScope loop ? loop : Parent?.GetEnclosingLoop();
    }

    public ImmutableArray<UseStatementSemantics> CollectUses(Scope? target)
    {
        var builder = ImmutableArray.CreateBuilder<UseStatementSemantics>();

        CollectScopedStatements(builder, static scope => scope.Uses, target);

        return builder.ToImmutable();
    }

    public ImmutableArray<DeferStatementSemantics> CollectDefers(Scope? target)
    {
        var builder = ImmutableArray.CreateBuilder<DeferStatementSemantics>();

        CollectScopedStatements(builder, static scope => scope.Defers, target);

        return builder.ToImmutable();
    }

    protected virtual void CollectScopedStatements<T>(
        ImmutableArray<T>.Builder builder, Func<BlockScope, ImmutableArray<T>.Builder> selector, Scope? target)
        where T : SemanticNode
    {
        if (this != target)
            Parent?.CollectScopedStatements(builder, selector, target);
    }

    public bool TryDefineSymbol(string name, Func<Symbol> creator, [MaybeNullWhen(true)] out Symbol existing)
    {
        ref var entry = ref CollectionsMarshal.GetValueRefOrAddDefault(_symbols, name, out _);

        if (entry != null)
        {
            existing = entry;

            return false;
        }

        existing = null;
        entry = creator();

        return true;
    }

    protected Symbol? ResolveLocalSymbol(string name)
    {
        return _symbols.GetValueOrDefault(name);
    }

    public virtual Symbol? ResolveSymbol(string name)
    {
        return ResolveLocalSymbol(name) is Symbol sym ? sym : Parent?.ResolveLocalSymbol(name);
    }
}
