namespace Vezel.Celerity.Semantics;

public abstract class CodeDeclaration : Declaration
{
    public new CodeDeclarationNode Syntax => Unsafe.As<CodeDeclarationNode>(base.Syntax);

    public ExpressionNode? Body => Syntax.Body;

    private protected CodeDeclaration(CodeDeclarationNode syntax)
        : base(syntax)
    {
    }
}
