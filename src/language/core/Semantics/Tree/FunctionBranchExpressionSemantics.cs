namespace Vezel.Celerity.Language.Semantics.Tree;

public abstract partial class FunctionBranchExpressionSemantics
{
    // These properties get set when the enclosing FunctionDeclarationSemantics or LambdaExpressionSemantics instance
    // (if any) is created, hence the declarations here rather than in SemanticTree.xml.

    public FunctionDeclarationSemantics? Function { get; internal set; }

    public LambdaExpressionSemantics? Lambda { get; internal set; }
}
