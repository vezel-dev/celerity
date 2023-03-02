using Vezel.Celerity.Language.Semantics.Tree;
using Vezel.Celerity.Language.Syntax.Tree;
using Vezel.Celerity.Language.Text;

namespace Vezel.Celerity.Language.Semantics.Binding;

public abstract class LocalSymbol : Symbol
{
    public override ImmutableArray<SemanticNode> Bindings => _bindings;

    public override ImmutableArray<IdentifierExpressionSemantics> References => _references;

    public override ImmutableArray<AssignmentExpressionSemantics> Assignments => _assignments;

    public override string Name { get; }

    private ImmutableArray<SemanticNode> _bindings = ImmutableArray<SemanticNode>.Empty;

    private ImmutableArray<IdentifierExpressionSemantics> _references =
        ImmutableArray<IdentifierExpressionSemantics>.Empty;

    private ImmutableArray<AssignmentExpressionSemantics> _assignments =
        ImmutableArray<AssignmentExpressionSemantics>.Empty;

    private protected LocalSymbol(string name)
    {
        Name = name;
    }

    public override IEnumerable<SourceLocation> GetLocations()
    {
        foreach (var binding in _bindings)
            yield return GetToken(binding).GetLocation();
    }

    private protected abstract SyntaxToken GetToken(SemanticNode node);

    private protected void AddBinding(SemanticNode node)
    {
        _bindings = _bindings.Add(node);
    }

    internal override void AddReference(IdentifierExpressionSemantics identifier)
    {
        _references = _references.Add(identifier);
    }

    internal override void AddAssignment(AssignmentExpressionSemantics assignment)
    {
        _assignments = _assignments.Add(assignment);
    }
}
