using Vezel.Celerity.Semantics.Tree;

namespace Vezel.Celerity.Semantics.Binding;

public sealed class DiscardSymbol : LocalSymbol
{
    internal DiscardSymbol()
    {
    }

    private protected override string GetName(SemanticNode node)
    {
        return Unsafe.As<PatternDiscardBindingSemantics>(node).Syntax.NameToken.Text;
    }

    internal void AddBinding(PatternDiscardBindingSemantics binding)
    {
        base.AddBinding(binding);
    }
}
