// SPDX-License-Identifier: 0BSD

namespace Vezel.Celerity.Language.Semantics.Tree;

public abstract partial class LoopBranchExpressionSemantics
{
    // This property gets set when the enclosing LoopExpressionSemantics instance (if any) is created, hence the
    // declaration here rather than in SemanticTree.xml.
    public LoopExpressionSemantics? Loop { get; internal set; }
}
