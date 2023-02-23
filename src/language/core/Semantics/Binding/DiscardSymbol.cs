using Vezel.Celerity.Language.Semantics.Tree;

namespace Vezel.Celerity.Language.Semantics.Binding;

public sealed class DiscardSymbol : LocalSymbol
{
    internal DiscardSymbol()
    {
    }

    private protected override string GetName(SemanticNode node)
    {
        return Unsafe.As<DiscardPatternBindingSemantics>(node).Syntax.NameToken.Text;
    }

    internal void AddBinding(DiscardPatternBindingSemantics binding)
    {
        base.AddBinding(binding);
    }
}
