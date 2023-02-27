using Vezel.Celerity.Language.Semantics.Tree;

namespace Vezel.Celerity.Language.Semantics.Binding;

public sealed class DeclarationSymbol : LocalSymbol, ILocalSymbol<DeclarationSymbol>
{
    private DeclarationSymbol()
    {
    }

    public static DeclarationSymbol Create()
    {
        return new();
    }

    private protected override string GetName(SemanticNode node)
    {
        return Unsafe.As<CodeDeclarationSemantics>(node).Syntax.NameToken.Text;
    }

    internal void AddBinding(CodeDeclarationSemantics declaration)
    {
        base.AddBinding(declaration);
    }
}
