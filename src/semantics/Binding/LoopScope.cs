using Vezel.Celerity.Semantics.Tree;

namespace Vezel.Celerity.Semantics.Binding;

internal sealed class LoopScope : Scope
{
    public ImmutableArray<LoopBranchExpressionSemantics>.Builder Branches { get; } =
        ImmutableArray.CreateBuilder<LoopBranchExpressionSemantics>();

    public LoopScope(Scope? parent)
        : base(parent)
    {
    }
}
