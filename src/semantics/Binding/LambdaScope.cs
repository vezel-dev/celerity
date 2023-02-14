namespace Vezel.Celerity.Semantics.Binding;

internal sealed class LambdaScope : Scope
{
    private readonly Dictionary<Symbol, UpvalueSymbol> _upvalues = new();

    private int _upvalueSlot;

    internal LambdaScope(Scope parent)
        : base(parent)
    {
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