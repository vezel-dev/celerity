using Vezel.Celerity.Semantics.Tree;

namespace Vezel.Celerity.Semantics.Binding;

public sealed class UpvalueSymbol : Symbol
{
    public override ImmutableArray<SemanticNode> Bindings => Parent.Bindings;

    public Symbol Parent { get; }

    public int Slot { get; }

    internal UpvalueSymbol(Symbol parent, int slot)
    {
        Parent = parent;
        Slot = slot;
    }

    private protected override string GetName(SemanticNode node)
    {
        return Parent.Name;
    }
}
