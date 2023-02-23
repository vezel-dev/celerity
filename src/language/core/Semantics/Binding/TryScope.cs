using Vezel.Celerity.Language.Semantics.Tree;

namespace Vezel.Celerity.Language.Semantics.Binding;

internal sealed class TryScope : Scope
{
    public ImmutableArray<CallExpressionSemantics>.Builder Calls { get; } =
        ImmutableArray.CreateBuilder<CallExpressionSemantics>();

    public ImmutableArray<RaiseExpressionSemantics>.Builder Raises { get; } =
        ImmutableArray.CreateBuilder<RaiseExpressionSemantics>();

    public TryScope(Scope? parent)
        : base(parent)
    {
    }
}
