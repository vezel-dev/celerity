namespace Vezel.Celerity.Semantics;

public sealed class FunctionDeclaration : CodeDeclaration
{
    public new FunctionDeclarationNode Syntax => Unsafe.As<FunctionDeclarationNode>(base.Syntax);

    public new BlockExpressionNode Body => Unsafe.As<BlockExpressionNode>(base.Body!);

    internal FunctionDeclaration(FunctionDeclarationNode syntax)
        : base(syntax)
    {
    }
}
