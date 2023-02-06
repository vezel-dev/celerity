namespace Vezel.Celerity.Semantics;

public sealed class LambdaScope : CallableScope
{
    public new LambdaExpressionNode Syntax => Unsafe.As<LambdaExpressionNode>(base.Syntax);

    public new Scope Parent => base.Parent!;

    public LambdaFunction Function { get; }

    public ImmutableDictionary<Symbol, UpvalueSymbol> Upvalues { get; private set; } =
        ImmutableDictionary<Symbol, UpvalueSymbol>.Empty;

    private int _slot;

    internal LambdaScope(LambdaFunction function, Scope parent)
        : base(function.Syntax, parent)
    {
        Function = function;
    }

    public override LoopScope? GetEnclosingLoop()
    {
        // Break/next expressions within a lambda cannot bind to loops outside of it.
        return null;
    }

    internal override Symbol? ResolveOrCaptureSymbol(string name)
    {
        // First check this lambda's own parameters and local variables without capturing.
        if (ResolveSymbol(name) is Symbol local)
            return local;

        // Next, resolve upwards...
        var sym = Parent.ResolveOrCaptureSymbol(name);

        // Does it exist at all?
        if (sym == null)
            return null;

        // We do not need to capture top-level declarations (functions and constants).
        if (sym is DeclarationSymbol)
            return sym;

        // We now know that the symbol is a parameter or local variable from further up the stack, possibly even
        // captured as an upvalue by an ancestor lambda. Register it as an upvalue of this lambda if that has not been
        // done already.
        if (!Upvalues.TryGetValue(sym, out var upvalue))
        {
            upvalue = new(this, _slot++, sym);

            // TODO: Use a collection builder behind the scenes.
            Upvalues = Upvalues.Add(sym, upvalue);
        }

        return upvalue;
    }
}
