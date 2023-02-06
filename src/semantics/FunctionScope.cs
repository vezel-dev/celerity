namespace Vezel.Celerity.Semantics;

public sealed class FunctionScope : CallableScope
{
    public new FunctionDeclarationNode Syntax => Unsafe.As<FunctionDeclarationNode>(base.Syntax);

    public new RootScope Parent => Unsafe.As<RootScope>(base.Syntax);

    public FunctionDeclaration Function { get; }

    internal FunctionScope(FunctionDeclaration function, RootScope parent)
        : base(function.Syntax, parent)
    {
        Function = function;
    }
}
