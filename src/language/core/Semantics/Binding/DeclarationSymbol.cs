using Vezel.Celerity.Language.Semantics.Tree;
using Vezel.Celerity.Language.Syntax.Tree;

namespace Vezel.Celerity.Language.Semantics.Binding;

public sealed class DeclarationSymbol : LocalSymbol, ILocalSymbol<DeclarationSymbol>
{
    public override bool IsMutable => false;

    public override bool IsDiscard => false;

    private DeclarationSymbol(string name)
        : base(name)
    {
    }

    static DeclarationSymbol ILocalSymbol<DeclarationSymbol>.Create(string name)
    {
        return new(name);
    }

    private protected override SyntaxToken GetToken(SemanticNode node)
    {
        return Unsafe.As<CodeDeclarationSemantics>(node).Syntax.NameToken;
    }

    internal void AddBinding(CodeDeclarationSemantics declaration)
    {
        base.AddBinding(declaration);
    }
}
