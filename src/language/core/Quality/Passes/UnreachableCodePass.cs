using Vezel.Celerity.Language.Semantics.Tree;
using Vezel.Celerity.Language.Text;

namespace Vezel.Celerity.Language.Quality.Passes;

public sealed class UnreachableCodePass : LintPass
{
    public static UnreachableCodePass Instance { get; } = new();

    private UnreachableCodePass()
        : base("unreachable-code")
    {
    }

    protected internal override void Run(LintPassContext context)
    {
        // TODO: This pass is currently very primitive. We can certainly do better with actual flow analysis.

        var descendants = context.Root.Descendants().ToArray();

        foreach (var block in descendants.OfType<BlockExpressionSemantics>())
        {
            var exited = false;
            var dead = new List<StatementSemantics>();

            foreach (var stmt in block.Statements)
            {
                if (exited)
                    dead.Add(stmt);
                else if (stmt is ExpressionStatementSemantics { Expression: BranchExpressionSemantics })
                    exited = true;
            }

            if (dead.Count != 0)
                context.ReportDiagnostic(
                    SourceTextSpan.Union(dead.First().Syntax.Span, dead.Last().Syntax.Span), "Code is unreachable");
        }

        // Only report a diagnostic if the try expression actually has a body and catch arms.
        foreach (var @try in descendants.OfType<TryExpressionSemantics>())
            if (@try is
                {
                    Raises.IsEmpty: true,
                    Calls.IsEmpty: true,
                    Syntax:
                    {
                        Body.Span.IsEmpty: false,
                        Arms.Span: { IsEmpty: false } span,
                    },
                })
                context.ReportDiagnostic(span, "Code is unreachable");
    }
}
