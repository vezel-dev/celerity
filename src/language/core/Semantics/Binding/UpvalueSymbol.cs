using Vezel.Celerity.Language.Semantics.Tree;

namespace Vezel.Celerity.Language.Semantics.Binding;

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
