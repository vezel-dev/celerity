namespace Vezel.Celerity.Semantics;

public sealed class LambdaScope : Scope
{
    public new LambdaExpressionNode Syntax => Unsafe.As<LambdaExpressionNode>(base.Syntax);

    internal LambdaScope(LambdaExpressionNode syntax, Scope? parent)
        : base(syntax, parent)
    {
    }
}
