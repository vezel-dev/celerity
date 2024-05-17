// SPDX-License-Identifier: 0BSD

using Vezel.Celerity.Language.Semantics.Tree;

namespace Vezel.Celerity.Language.Semantics.Binding;

internal sealed class LoopScope : Scope, IScope<LoopScope>
{
    public ImmutableArray<LoopBranchExpressionSemantics>.Builder BranchExpressions { get; } =
        ImmutableArray.CreateBuilder<LoopBranchExpressionSemantics>();

    private LoopScope(Scope? parent)
        : base(parent)
    {
    }

    static LoopScope IScope<LoopScope>.Create(Scope? parent)
    {
        return new(parent);
    }
}
