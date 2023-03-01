using Vezel.Celerity.Language.Semantics.Tree;

namespace Vezel.Celerity.Language.Semantics.Binding;

internal sealed class TryScope : Scope, IScope<TryScope>
{
    public ImmutableArray<CallExpressionSemantics>.Builder Calls { get; } =
        ImmutableArray.CreateBuilder<CallExpressionSemantics>();

    public ImmutableArray<RaiseExpressionSemantics>.Builder Raises { get; } =
        ImmutableArray.CreateBuilder<RaiseExpressionSemantics>();

    private TryScope(Scope? parent)
        : base(parent)
    {
    }

    static TryScope IScope<TryScope>.Create(Scope? parent)
    {
        return new(parent);
    }
}
