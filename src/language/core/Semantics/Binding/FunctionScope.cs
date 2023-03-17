using Vezel.Celerity.Language.Semantics.Tree;

namespace Vezel.Celerity.Language.Semantics.Binding;

internal class FunctionScope : Scope, IScope<FunctionScope>
{
    public bool IsFallible { get; set; }

    public ImmutableArray<CallExpressionSemantics>.Builder CallExpressions { get; } =
        ImmutableArray.CreateBuilder<CallExpressionSemantics>();

    public ImmutableArray<RaiseExpressionSemantics>.Builder RaiseExpressions { get; } =
        ImmutableArray.CreateBuilder<RaiseExpressionSemantics>();

    protected internal FunctionScope(Scope? parent)
        : base(parent)
    {
    }

    static FunctionScope IScope<FunctionScope>.Create(Scope? parent)
    {
        return new(parent);
    }
}