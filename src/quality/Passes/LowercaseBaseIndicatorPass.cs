namespace Vezel.Celerity.Quality.Passes;

public sealed class LowercaseBaseIndicatorPass : LintPass
{
    public static LowercaseBaseIndicatorPass Instance { get; } = new();

    private LowercaseBaseIndicatorPass()
        : base(
            "lowercase-base-indicator",
            SourceDiagnosticSeverity.Warning,
            LintTargets.Expression | LintTargets.Pattern,
            SyntaxMode.Module)
    {
    }

    protected internal override void Run(LintContext context, ExpressionSemantics expression)
    {
        if (expression is LiteralExpressionSemantics { Syntax.LiteralToken: var token })
            CheckIntegerLiteral(context, token);
    }

    protected internal override void Run(LintContext context, PatternSemantics pattern)
    {
        if (pattern is LiteralPatternSemantics { Syntax.LiteralToken: var token })
            CheckIntegerLiteral(context, token);
    }

    [SuppressMessage("", "CA1308")]
    private static void CheckIntegerLiteral(LintContext context, SyntaxToken token)
    {
        var text = token.Text;

        if (token.Kind != SyntaxTokenKind.IntegerLiteral ||
            text.Length < 2 ||
            text.AsSpan(0, 2) is not ("0B" or "0O" or "0X"))
            return;

        context.CreateDiagnostic(
            token.Location, $"Consider using lowercase base indicator '{text[..2].ToLowerInvariant()}' for clarity");
    }
}
