namespace Vezel.Celerity.Semantics;

public sealed class FunctionScope : DeclarationScope
{
    public new FunctionDeclarationNode Syntax => Unsafe.As<FunctionDeclarationNode>(base.Syntax);

    internal FunctionScope(FunctionDeclarationNode syntax)
        : base(syntax)
    {
    }
}
