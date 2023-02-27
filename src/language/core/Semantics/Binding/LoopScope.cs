using Vezel.Celerity.Language.Semantics.Tree;

namespace Vezel.Celerity.Language.Semantics.Binding;

internal sealed class LoopScope : Scope, IScope<LoopScope>
{
    public ImmutableArray<LoopBranchExpressionSemantics>.Builder Branches { get; } =
        ImmutableArray.CreateBuilder<LoopBranchExpressionSemantics>();

    private LoopScope(Scope? parent)
        : base(parent)
    {
    }

    public static new LoopScope Create(Scope? parent)
    {
        return new(parent);
    }
}
