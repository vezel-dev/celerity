namespace Vezel.Celerity.Semantics;

public sealed class BlockScope : Scope
{
    public new BlockExpressionNode Syntax => Unsafe.As<BlockExpressionNode>(base.Syntax);

    public new Scope Parent => base.Parent!;

    public ImmutableArray<UseStatementNode> Uses { get; private set; } = ImmutableArray<UseStatementNode>.Empty;

    public ImmutableArray<DeferStatementNode> Defers { get; private set; } = ImmutableArray<DeferStatementNode>.Empty;

    internal BlockScope(BlockExpressionNode syntax, Scope parent)
        : base(syntax, parent)
    {
    }

    internal void AddUse(UseStatementNode statement)
    {
        // TODO: Use a collection builder behind the scenes.
        Uses = Uses.Add(statement);
    }

    internal void AddDefer(DeferStatementNode statement)
    {
        // TODO: Use a collection builder behind the scenes.
        Defers = Defers.Add(statement);
    }
}
