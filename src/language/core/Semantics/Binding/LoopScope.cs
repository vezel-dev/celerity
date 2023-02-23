using Vezel.Celerity.Language.Semantics.Tree;

namespace Vezel.Celerity.Language.Semantics.Binding;

internal sealed class LoopScope : Scope
{
    public ImmutableArray<LoopBranchExpressionSemantics>.Builder Branches { get; } =
        ImmutableArray.CreateBuilder<LoopBranchExpressionSemantics>();

    public LoopScope(Scope? parent)
        : base(parent)
    {
    }
}
