namespace Vezel.Celerity.Semantics;

public sealed class UpvalueSymbol : Symbol
{
    public int Slot { get; }

    public Symbol Symbol { get; }

    public override string Name => Symbol.Name;

    internal UpvalueSymbol(int slot, Symbol symbol)
        : base(symbol.Syntax)
    {
        Slot = slot;
        Symbol = symbol;
    }
}
