// SPDX-License-Identifier: 0BSD

namespace Vezel.Celerity.Language.Semantics.Tree;

public sealed partial class ThisExpressionSemantics
{
    // This property gets set when the enclosing LambdaExpressionSemantics instance (if any) is created, hence the
    // declaration here rather than in SemanticTree.xml.
    public LambdaExpressionSemantics? Lambda { get; internal set; }
}
