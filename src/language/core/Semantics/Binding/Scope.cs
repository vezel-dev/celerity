using Vezel.Celerity.Language.Semantics.Tree;

namespace Vezel.Celerity.Language.Semantics.Binding;

internal class Scope : IScope<Scope>
{
    public Scope? Parent { get; }

    private readonly Dictionary<string, Symbol> _symbols = new();

    protected Scope(Scope? parent)
    {
        Parent = parent;
    }

    public static Scope Create(Scope? parent)
    {
        return new(parent);
    }

    public virtual TryScope? GetEnclosingTry()
    {
        // Consider:
        //
        // try raise err { } catch {
        //     _ => 42,
        // };
        //
        // When we get to the raise expression, the top of the scope stack will be the TryScope for the try expression.
        // That is the one we want to bind to, so no special consideration is needed here.
        return this is TryScope @try ? @try : Parent?.GetEnclosingTry();
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

    public ImmutableArray<DeferStatementSemantics> CollectDefers(Scope? target)
    {
        var builder = ImmutableArray.CreateBuilder<DeferStatementSemantics>();

        CollectDefers(target, builder);

        return builder.DrainToImmutable();
    }

    protected virtual void CollectDefers(Scope? target, ImmutableArray<DeferStatementSemantics>.Builder builder)
    {
        if (this != target)
            Parent?.CollectDefers(target, builder);
    }

    public bool TryDefineSymbol<T>(string name, [MaybeNullWhen(true)] out Symbol existing)
        where T : LocalSymbol, ILocalSymbol<T>
    {
        ref var entry = ref CollectionsMarshal.GetValueRefOrAddDefault(_symbols, name, out _);

        if (entry != null)
        {
            existing = entry;

            return false;
        }

        entry = T.Create();
        existing = null;

        return true;
    }

    protected Symbol? ResolveLocalSymbol(string name)
    {
        return _symbols.GetValueOrDefault(name);
    }

    public virtual Symbol? ResolveSymbol(string name)
    {
        return ResolveLocalSymbol(name) ?? Parent?.ResolveLocalSymbol(name);
    }
}
