namespace Vezel.Celerity.Semantics;

public sealed class LoopScope : Scope
{
    public new LoopExpressionNode Syntax => Unsafe.As<LoopExpressionNode>(base.Syntax);

    public new Scope Parent => base.Parent!;

    public ImmutableArray<NextExpressionNode> Nexts { get; private set; } =
        ImmutableArray<NextExpressionNode>.Empty;

    public ImmutableArray<BreakExpressionNode> Breaks { get; private set; } =
        ImmutableArray<BreakExpressionNode>.Empty;

    internal LoopScope(LoopExpressionNode syntax, Scope parent)
        : base(syntax, parent)
    {
    }

    internal void AddNext(NextExpressionNode expression)
    {
        // TODO: Use a collection builder behind the scenes.
        Nexts = Nexts.Add(expression);
    }

    internal void AddBreak(BreakExpressionNode expression)
    {
        // TODO: Use a collection builder behind the scenes.
        Breaks = Breaks.Add(expression);
    }
}
