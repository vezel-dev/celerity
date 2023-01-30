namespace Vezel.Celerity.Semantics;

public sealed class ConstantDeclaration : CodeDeclaration
{
    public new ConstantDeclarationNode Syntax => Unsafe.As<ConstantDeclarationNode>(base.Syntax);

    public new ExpressionNode Body => base.Body!;

    internal ConstantDeclaration(ConstantDeclarationNode syntax)
        : base(syntax)
    {
    }
}
