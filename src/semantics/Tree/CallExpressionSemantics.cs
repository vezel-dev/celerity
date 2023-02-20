namespace Vezel.Celerity.Semantics.Tree;

public sealed partial class CallExpressionSemantics
{
    // This property gets set when the enclosing TryExpressionSemantics instance (if any) is created, hence the
    // declaration here rather than in SemanticTree.xml.
    public TryExpressionSemantics? Try { get; internal set; }
}
