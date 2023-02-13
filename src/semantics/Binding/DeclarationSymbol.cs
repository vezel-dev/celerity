using Vezel.Celerity.Semantics.Tree;

namespace Vezel.Celerity.Semantics.Binding;

public sealed class DeclarationSymbol : LocalSymbol
{
    internal DeclarationSymbol()
    {
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
