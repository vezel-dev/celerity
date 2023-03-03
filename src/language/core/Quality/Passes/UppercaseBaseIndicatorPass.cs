using Vezel.Celerity.Language.Diagnostics;
using Vezel.Celerity.Language.Semantics.Tree;
using Vezel.Celerity.Language.Syntax;
using Vezel.Celerity.Language.Syntax.Tree;

namespace Vezel.Celerity.Language.Quality.Passes;

public sealed class UppercaseBaseIndicatorPass : LintPass
{
    public static UppercaseBaseIndicatorPass Instance { get; } = new();

    private UppercaseBaseIndicatorPass()
        : base(
            "uppercase-base-indicator",
            DiagnosticSeverity.Warning,
            LintTargets.Expression | LintTargets.Pattern,
            SyntaxMode.Module)
    {
    }

    protected internal override void Run(LintContext context, ExpressionSemantics expression)
    {
        if (expression is LiteralExpressionSemantics literal)
            CheckIntegerLiteral(context, literal.Syntax.LiteralToken);
    }

    protected internal override void Run(LintContext context, PatternSemantics pattern)
    {
        if (pattern is LiteralPatternSemantics literal)
            CheckIntegerLiteral(context, literal.Syntax.LiteralToken);
    }

    [SuppressMessage("", "CA1308")]
    private static void CheckIntegerLiteral(LintContext context, SyntaxToken token)
    {
        var text = token.Text;

        if (token.Kind == SyntaxTokenKind.IntegerLiteral &&
            text.Length >= 2 &&
            text.AsSpan(0, 2) is "0B" or "0O" or "0X")
            context.ReportDiagnostic(
                token.Span, $"Consider using lowercase base indicator '{text[..2].ToLowerInvariant()}' for clarity");
    }
}
