namespace Vezel.Celerity.Semantics;

public sealed class TestDeclaration : CodeDeclaration
{
    public new TestDeclarationNode Syntax => Unsafe.As<TestDeclarationNode>(base.Syntax);

    public new BlockExpressionNode Body => Unsafe.As<BlockExpressionNode>(base.Body!);

    internal TestDeclaration(TestDeclarationNode syntax)
        : base(syntax)
    {
    }
}
