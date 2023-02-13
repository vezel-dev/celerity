namespace Vezel.Celerity.Semantics.Tree;

public abstract partial class LoopBranchExpressionSemantics
{
    // This property gets set when the enclosing LoopExpressionSemantics instance is created, hence the declaration here
    // rather than in SemanticTree.xml.
    public LoopExpressionSemantics? Loop { get; internal set; }
}
