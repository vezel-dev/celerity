using Vezel.Celerity.Semantics.Tree;

namespace Vezel.Celerity.Semantics.Binding;

public sealed class VariableSymbol : LocalSymbol
{
    public override bool IsMutable =>
        Bindings.Any(decl => decl is PatternVariableBindingSemantics { Syntax.MutKeywordToken: not null });

    internal VariableSymbol()
    {
    }

    private protected override string GetName(SemanticNode node)
    {
        return Unsafe.As<PatternVariableBindingSemantics>(node).Syntax.NameToken.Text;
    }

    internal void AddBinding(PatternVariableBindingSemantics binding)
    {
        base.AddBinding(binding);
    }
}
