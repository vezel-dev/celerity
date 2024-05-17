// SPDX-License-Identifier: 0BSD

namespace Vezel.Celerity.Language.Semantics.Tree;

public sealed partial class CallExpressionSemantics
{
    // These properties get set when the enclosing FunctionDeclarationSemantics, LambdaExpressionSemantics, or
    // TryExpressionSemantics instance (if any) is created, hence the declarations here rather than in SemanticTree.xml.

    public FunctionDeclarationSemantics? Function { get; internal set; }

    public LambdaExpressionSemantics? Lambda { get; internal set; }

    public TryExpressionSemantics? Try { get; internal set; }
}
