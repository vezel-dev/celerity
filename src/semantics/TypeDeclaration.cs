namespace Vezel.Celerity.Semantics;

public sealed class TypeDeclaration : Declaration
{
    public new TypeDeclarationNode Syntax => Unsafe.As<TypeDeclarationNode>(base.Syntax);

    internal TypeDeclaration(TypeDeclarationNode syntax)
        : base(syntax)
    {
    }
}
