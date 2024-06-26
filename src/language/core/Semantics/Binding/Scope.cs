// SPDX-License-Identifier: 0BSD

using Vezel.Celerity.Language.Semantics.Tree;

namespace Vezel.Celerity.Language.Semantics.Binding;

internal class Scope : IScope<Scope>
{
    public Scope? Parent { get; }

    private readonly Dictionary<string, Symbol> _symbols = [];

    protected internal Scope(Scope? parent)
    {
        Parent = parent;
    }

    static Scope IScope<Scope>.Create(Scope? parent)
    {
        return new(parent);
    }

    public virtual FunctionScope? GetEnclosingFunction(bool ignoreDefer)
    {
        // Consider:
        //
        // fn() -> this();
        //
        // When we get to the this expression, the top of the scope stack will be the LambdaScope for the lambda
        // expression. That is the one we want to bind to, so no special consideration is needed here.
        //
        // Also consider:
        //
        // fn() -> fn() -> this();
        //
        // In this case, the this expression should bind to the inner lambda expression; there is no way to refer to an
        // outer lambda expression with a this expression.
        return this is FunctionScope function ? function : Parent?.GetEnclosingFunction(ignoreDefer);
    }

    public virtual TryScope? GetEnclosingTry()
    {
        // Consider:
        //
        // try raise err FooError { } catch {
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
        //         42;
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

    public bool DefineSymbol<T>(string name, out Symbol result)
        where T : LocalSymbol, ILocalSymbol<T>
    {
        result = CollectionsMarshal.GetValueRefOrAddDefault(_symbols, name, out var exists) ??= T.Create(name);

        return !exists;
    }

    protected Symbol? ResolveLocalSymbol(string name)
    {
        return _symbols.GetValueOrDefault(name);
    }

    public virtual Symbol? ResolveSymbol(string name)
    {
        return ResolveLocalSymbol(name) ?? Parent?.ResolveSymbol(name);
    }
}
