namespace Vezel.Celerity.Language.Semantics.Tree;

public sealed partial class RaiseExpressionSemantics
{
    // This property gets set when the enclosing TryExpressionSemantics instance (if any) is created, hence the
    // declaration here rather than in SemanticTree.xml.
    public TryExpressionSemantics? Try { get; internal set; }
}
