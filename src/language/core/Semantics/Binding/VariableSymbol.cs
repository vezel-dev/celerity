using Vezel.Celerity.Language.Semantics.Tree;

namespace Vezel.Celerity.Language.Semantics.Binding;

public sealed class VariableSymbol : LocalSymbol, ILocalSymbol<VariableSymbol>
{
    public override bool IsMutable =>
        Bindings.Any(decl => decl is VariableBindingSemantics { Syntax.MutKeywordToken: not null });

    public bool IsDiscard => Name[0] == '_';

    private VariableSymbol()
    {
    }

    public static VariableSymbol Create()
    {
        return new();
    }

    private protected override string GetName(SemanticNode node)
    {
        return Unsafe.As<BindingSemantics>(node).Syntax.NameToken.Text;
    }

    internal void AddBinding(BindingSemantics binding)
    {
        base.AddBinding(binding);
    }
}
