using Vezel.Celerity.Language.Semantics.Tree;
using Vezel.Celerity.Language.Syntax.Tree;
using Vezel.Celerity.Language.Text;

namespace Vezel.Celerity.Language.Semantics.Binding;

public abstract class LocalSymbol : Symbol
{
    public override sealed string Name { get; }

    public override sealed ImmutableArray<SemanticNode> Bindings => _bindings;

    public override sealed ImmutableArray<IdentifierExpressionSemantics> References => _references;

    public override sealed ImmutableArray<AssignmentExpressionSemantics> Assignments => _assignments;

    private ImmutableArray<SemanticNode> _bindings = [];

    private ImmutableArray<IdentifierExpressionSemantics> _references = [];

    private ImmutableArray<AssignmentExpressionSemantics> _assignments = [];

    private protected LocalSymbol(string name)
    {
        Name = name;
    }

    public override sealed IEnumerable<SourceTextSpan> GetSpans()
    {
        foreach (var binding in _bindings)
            yield return GetToken(binding).Span;
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
