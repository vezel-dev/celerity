using Vezel.Celerity.Language.Semantics.Tree;

namespace Vezel.Celerity.Language.Semantics.Binding;

internal sealed class LambdaScope : Scope, IScope<LambdaScope>
{
    public ImmutableArray<ThisExpressionSemantics>.Builder ThisExpressions { get; } =
        ImmutableArray.CreateBuilder<ThisExpressionSemantics>(0);

    private readonly Dictionary<Symbol, UpvalueSymbol> _upvalues = new();

    private int _upvalueSlot;

    private LambdaScope(Scope? parent)
        : base(parent)
    {
    }

    static LambdaScope IScope<LambdaScope>.Create(Scope? parent)
    {
        return new(parent);
    }

    public override TryScope? GetEnclosingTry()
    {
        // We might be analyzing code like this:
        //
        // try {
        //     let func = fn() -> raise err { };
        //     func();
        // } catch {
        //     _ => 42,
        // };
        //
        // The raise expression in the lambda expression should cause the error to be returned to the unexpectant
        // caller, causing a panic in this case. The raise should not bind to the outer try expression. So we
        // short-circuit the scope walk by returning null here.
        return null;
    }

    public override LoopScope? GetEnclosingLoop()
    {
        // We might be analyzing code like this:
        //
        // while true {
        //     let func = fn() -> break;
        //     func();
        // };
        //
        // This is erroneous; a break/next expression in a lambda expression cannot bind to a loop outside of the lambda
        // expression. So we short-circuit the scope walk by returning null here.
        return null;
    }

    protected override void CollectDefers(Scope? target, ImmutableArray<DeferStatementSemantics>.Builder builder)
    {
        // Consider this code:
        //
        // defer foo();
        // let func = fn() -> {
        //     defer bar();
        //     42;
        // };
        // func();
        // while true {
        //     // Spin forever...
        // };
        //
        // When func returns, only the inner defer (i.e. the bar call) should run. We short-circuit the scope walk here
        // to achieve that.
    }

    public override Symbol? ResolveSymbol(string name)
    {
        // A few cases to consider:
        //
        // let x = 42;
        // let f = fn() -> x;
        //
        // Here, x needs to be captured as an upvalue.
        //
        // let g = fn(y) -> y;
        //
        // Here, y is a parameter of the lambda expression, so no capturing is required.
        //
        // pub fn foo() {
        //     fn() -> foo;
        // }
        //
        // Here, foo is a top-level declaration, so no capturing is required.
        //
        // let z = 21;
        // let h = fn() -> {
        //     fn() -> z;
        // };
        //
        // Here, z needs to be captured as an upvalue of the outer lambda expression. That upvalue then needs to be
        // captured as an upvalue of the inner lambda expression. In other words, upvalues can be chained.

        // First check this lambda's symbols (i.e. parameters) without capturing.
        if (ResolveLocalSymbol(name) is { } local)
            return local;

        // The symbol must come from further up the scope tree, so resolve upwards.
        if (Parent!.ResolveSymbol(name) is not { } sym)
            return null;

        // Declarations do not live on the call stack, so we do not need to capture them as upvalues.
        if (sym is DeclarationSymbol)
            return sym;

        // We now know that the symbol is a parameter or local variable from further up the call stack, possibly even
        // captured as an upvalue by an ancestor lambda. Register it as an upvalue of this lambda if that has not been
        // done already.
        ref var entry = ref CollectionsMarshal.GetValueRefOrAddDefault(_upvalues, sym, out _);

        entry ??= new(sym, _upvalueSlot++);

        return entry;
    }
}
