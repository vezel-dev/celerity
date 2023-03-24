using Vezel.Celerity.Language.Diagnostics;
using Vezel.Celerity.Language.Semantics.Tree;
using Vezel.Celerity.Language.Text;

namespace Vezel.Celerity.Language.Quality.Passes;

public sealed class UnreachableCodePass : LintPass
{
    public static UnreachableCodePass Instance { get; } = new();

    private UnreachableCodePass()
        : base("unreachable-code", DiagnosticSeverity.Warning, null)
    {
    }

    protected internal override void Run(LintContext context)
    {
        // TODO: This pass is currently very primitive. We can certainly do better with actual flow analysis.

        foreach (var block in context.Tree.Root.Descendants().OfType<BlockExpressionSemantics>())
        {
            var exited = false;
            var dead = new List<StatementSemantics>();

            foreach (var stmt in block.Statements)
            {
                if (exited)
                    dead.Add(stmt);
                else if (stmt is ExpressionStatementSemantics
                {
                    Expression: BranchExpressionSemantics or LoopBranchExpressionSemantics,
                })
                    exited = true;
            }

            if (dead.Count != 0)
                context.ReportDiagnostic(
                    SourceTextSpan.Union(dead.First().Syntax.Span, dead.Last().Syntax.Span), "Code is unreachable");
        }
    }
}
