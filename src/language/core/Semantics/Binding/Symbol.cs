using Vezel.Celerity.Language.Semantics.Tree;
using Vezel.Celerity.Language.Text;

namespace Vezel.Celerity.Language.Semantics.Binding;

public abstract class Symbol
{
    public abstract ImmutableArray<SemanticNode> Bindings { get; }

    public abstract ImmutableArray<AssignmentExpressionSemantics> Assignments { get; }

    public abstract string Name { get; }

    public abstract bool IsMutable { get; }

    public abstract bool IsDiscard { get; }

    private protected Symbol()
    {
    }

    public abstract IEnumerable<SourceLocation> GetLocations();

    internal abstract void AddAssignment(AssignmentExpressionSemantics assignment);
}