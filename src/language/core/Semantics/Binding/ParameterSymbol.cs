using Vezel.Celerity.Language.Semantics.Tree;
using Vezel.Celerity.Language.Syntax.Tree;

namespace Vezel.Celerity.Language.Semantics.Binding;

public sealed class ParameterSymbol : LocalSymbol, ILocalSymbol<ParameterSymbol>
{
    public override bool IsMutable => false;

    public override bool IsDiscard => Name[0] == '_';

    private ParameterSymbol()
    {
    }

    public static ParameterSymbol Create()
    {
        return new();
    }

    private protected override SyntaxToken GetToken(SemanticNode node)
    {
        return Unsafe.As<CodeDeclarationSemantics>(node).Syntax.NameToken;
    }

    internal void AddBinding(CodeParameterSemantics parameter)
    {
        base.AddBinding(parameter);
    }
}
