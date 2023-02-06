namespace Vezel.Celerity.Semantics;

public sealed class UpvalueSymbol : Symbol
{
    public new LambdaScope Scope => Unsafe.As<LambdaScope>(base.Scope);

    public int Slot { get; }

    public Symbol Symbol { get; }

    public override string Name => Symbol.Name;

    internal UpvalueSymbol(LambdaScope scope, int slot, Symbol symbol)
        : base(symbol.Syntax, scope)
    {
        Slot = slot;
        Symbol = symbol;
    }
}
