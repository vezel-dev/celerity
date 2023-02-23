using Vezel.Celerity.Language.Semantics.Tree;

namespace Vezel.Celerity.Language.Semantics.Binding;

public sealed class VariableSymbol : LocalSymbol
{
    public override bool IsMutable =>
        Bindings.Any(decl => decl is VariablePatternBindingSemantics { Syntax.MutKeywordToken: not null });

    internal VariableSymbol()
    {
    }

    private protected override string GetName(SemanticNode node)
    {
        return Unsafe.As<VariablePatternBindingSemantics>(node).Syntax.NameToken.Text;
    }

    internal void AddBinding(VariablePatternBindingSemantics binding)
    {
        base.AddBinding(binding);
    }
}
