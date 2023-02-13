using Vezel.Celerity.Semantics.Tree;

namespace Vezel.Celerity.Semantics.Binding;

public abstract class LocalSymbol : Symbol
{
    public override ImmutableArray<SemanticNode> Bindings => _bindings;

    private ImmutableArray<SemanticNode> _bindings = ImmutableArray<SemanticNode>.Empty;

    private protected LocalSymbol()
    {
    }

    private protected void AddBinding(SemanticNode node)
    {
        _bindings = _bindings.Add(node);
    }
}
