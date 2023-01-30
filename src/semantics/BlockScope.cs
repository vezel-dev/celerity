namespace Vezel.Celerity.Semantics;

public sealed class BlockScope : Scope
{
    public new BlockExpressionNode Syntax => Unsafe.As<BlockExpressionNode>(base.Syntax);

    internal BlockScope(BlockExpressionNode syntax, Scope? parent)
        : base(syntax, parent)
    {
    }
}
