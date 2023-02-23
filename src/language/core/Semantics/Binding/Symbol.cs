using Vezel.Celerity.Language.Semantics.Tree;

namespace Vezel.Celerity.Language.Semantics.Binding;

public abstract class Symbol
{
    public abstract ImmutableArray<SemanticNode> Bindings { get; }

    public ImmutableArray<AssignmentExpressionSemantics> Assignments { get; private set; }

    public string Name => GetName(Bindings[0]);

    public virtual bool IsMutable => false;

    private protected Symbol()
    {
    }

    private protected abstract string GetName(SemanticNode node);

    internal void AddAssignment(AssignmentExpressionSemantics assignment)
    {
        Assignments = Assignments.Add(assignment);
    }
}
