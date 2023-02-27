using Vezel.Celerity.Language.Semantics.Tree;

namespace Vezel.Celerity.Language.Semantics.Binding;

public sealed class ParameterSymbol : LocalSymbol, ILocalSymbol<ParameterSymbol>
{
    public bool IsDiscard => Name[0] == '_';

    private ParameterSymbol()
    {
    }

    public static ParameterSymbol Create()
    {
        return new();
    }

    private protected override string GetName(SemanticNode node)
    {
        return Unsafe.As<CodeParameterSemantics>(node).Syntax.NameToken.Text;
    }

    internal void AddBinding(CodeParameterSemantics parameter)
    {
        base.AddBinding(parameter);
    }
}
