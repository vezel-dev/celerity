using Vezel.Celerity.Language.Semantics.Tree;
using Vezel.Celerity.Language.Syntax.Tree;

namespace Vezel.Celerity.Language.Semantics.Binding;

public sealed class ParameterSymbol : LocalSymbol, ILocalSymbol<ParameterSymbol>
{
    public override bool IsMutable => false;

    public override bool IsDiscard => Name[0] == '_';

    private ParameterSymbol(string name)
        : base(name)
    {
    }

    static ParameterSymbol ILocalSymbol<ParameterSymbol>.Create(string name)
    {
        return new(name);
    }

    private protected override SyntaxToken GetToken(SemanticNode node)
    {
        return Unsafe.As<CodeParameterSemantics>(node).Syntax.NameToken;
    }

    internal void AddBinding(CodeParameterSemantics parameter)
    {
        base.AddBinding(parameter);
    }
}
