using Vezel.Celerity.Language.Semantics.Tree;
using Vezel.Celerity.Language.Text;

namespace Vezel.Celerity.Language.Semantics.Binding;

public sealed class UpvalueSymbol : Symbol
{
    public Symbol Parent { get; }

    public int Slot { get; }

    public override ImmutableArray<SemanticNode> Bindings => Parent.Bindings;

    public override ImmutableArray<AssignmentExpressionSemantics> Assignments => Parent.Assignments;

    public override string Name => Parent.Name;

    public override bool IsMutable => false; // Upvalue variables can never be mutated.

    public override bool IsDiscard => Parent.IsDiscard;

    internal UpvalueSymbol(Symbol parent, int slot)
    {
        Parent = parent;
        Slot = slot;
    }

    public override IEnumerable<SourceLocation> GetLocations()
    {
        return Parent.GetLocations();
    }

    internal override void AddAssignment(AssignmentExpressionSemantics assignment)
    {
        Parent.AddAssignment(assignment);
    }
}