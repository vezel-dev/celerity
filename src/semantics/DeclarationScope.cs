namespace Vezel.Celerity.Semantics;

public class DeclarationScope : Scope
{
    public new CodeDeclarationNode Syntax => Unsafe.As<CodeDeclarationNode>(base.Syntax);

    internal DeclarationScope(CodeDeclarationNode syntax)
        : base(syntax, null)
    {
    }
}
