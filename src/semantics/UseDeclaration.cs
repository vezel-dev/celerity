namespace Vezel.Celerity.Semantics;

public sealed class UseDeclaration : Declaration
{
    public new UseDeclarationNode Syntax => Unsafe.As<UseDeclarationNode>(base.Syntax);

    internal UseDeclaration(UseDeclarationNode syntax)
        : base(syntax)
    {
    }
}
